using LunchBag.Common.IoTMessages;
using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.SimulatorApp.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace LunchBag.SimulatorApp
{
    public class Application : IDisposable
    {
        private readonly ILogger<Application> _logger;
        private readonly AzureServiceBusPublisher _azureServiceBusPublisherButtons;
        private readonly AzureServiceBusPublisher _azureServiceBusPublisherNotes;
        private readonly ICosmosDBRepository<EventModel> _repoEvents;
        private readonly ICosmosDBRepository<NoteTemplateModel> _repoNoteTemplates;
        private readonly SimulationConfig _config;

        private Random _rand = new Random();
        private List<Timer> _timers = new List<Timer>();
        private List<NoteTemplateModel> _nodeTemplates = null;

        public Application(ILogger<Application> logger, 
            IOptions<SimulationConfig> config,
            IEnumerable<AzureServiceBusPublisher> azureServiceBusPublisher,
            ICosmosDBRepository<EventModel> repoEvents,
            ICosmosDBRepository<NoteTemplateModel> repoNoteTemplates)
        {
            _logger = logger;
            _config = config.Value;
            _repoEvents = repoEvents;
            _repoNoteTemplates = repoNoteTemplates;
            foreach (var azureServiceBus in azureServiceBusPublisher)
            {
                if (azureServiceBus.Name == "buttons")
                    _azureServiceBusPublisherButtons = azureServiceBus;
                else if (azureServiceBus.Name == "notes")
                    _azureServiceBusPublisherNotes = azureServiceBus;
                azureServiceBus.Start();
            }
        }

        public async Task Run()
        {
            try
            {
                //Load event to test
                EventModel vEvent = await _repoEvents.GetItemAsync(_config.EventId);

                //Load note templates
                IEnumerable<NoteTemplateModel> noteTemplatesEnum = await _repoNoteTemplates.GetItemsAsync();
                _nodeTemplates = new List<NoteTemplateModel>();
                foreach(var noteTemplate in noteTemplatesEnum)
                    _nodeTemplates.Add(noteTemplate);

                //Set up timers for buttons / notes
                foreach (EventLocationModel eventLocation in vEvent.EventLocations)
                {
                    Timer newTimer = new Timer();
                    newTimer.Elapsed += (sender, e) => SendButtonMessage(sender, e, newTimer, vEvent.Id, eventLocation.Id);
                    newTimer.Interval = _rand.Next(_config.TimerMin, _config.TimerMax);
                    newTimer.Start();
                    _timers.Add(newTimer);

                    Timer newTimerNotes = new Timer();
                    newTimerNotes.Elapsed += (sender, e) => SendNoteMessage(sender, e, newTimerNotes, vEvent.Id, eventLocation.Id);
                    newTimerNotes.Interval = _rand.Next(_config.TimerMin, _config.TimerMax);
                    newTimerNotes.Start();
                    _timers.Add(newTimerNotes);
                }

                Console.WriteLine("Press a key to exit.");
                Console.Read();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private async void SendButtonMessage(object source, ElapsedEventArgs e, Timer currentTimer, string eventId, string locationId)
        {
            currentTimer.Interval = _rand.Next(_config.TimerMin, _config.TimerMax);
            await _azureServiceBusPublisherButtons.Publish(new ButtonTriggerIoTMessage()
            {
                EventId = eventId,
                LocationId = locationId,
                Capacity = _rand.Next(10, 25),
                UniqueId = Guid.NewGuid().ToString(),
                MessageType = IoTMessageType.Button
            });
        }

        private async void SendNoteMessage(object source, ElapsedEventArgs e, Timer currentTimer, string eventId, string locationId)
        {
            currentTimer.Interval = _rand.Next(_config.TimerMin, _config.TimerMax);

            if (_nodeTemplates.Count == 0)
                return;

            //Choose template randomly
            NoteTemplateModel nodeTemplate = _nodeTemplates[_rand.Next(0, _nodeTemplates.Count - 1)];

            await _azureServiceBusPublisherNotes.Publish(new NoteCreatedIoTMessage()
            {
                EventId = eventId,
                Note = nodeTemplate.Note,
                MessageType = IoTMessageType.Note
            });
        }

        public void Dispose()
        {
            if(_timers != null)
            {
                foreach(var timer in _timers)
                {
                    if(timer != null)
                    {
                        if (timer.Enabled)
                            timer.Stop();
                        timer.Dispose();
                    }
                }
            }
        }
    }
}
