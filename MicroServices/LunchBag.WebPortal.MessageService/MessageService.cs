using LunchBag.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using LunchBag.Common.Managers;
using RestSharp;
using LunchBag.Common.EventMessages;
using System.Net;
using LunchBag.Common.IoTMessages;
using F23.StringSimilarity;
using LunchBag.Common.Config;
using LunchBag.WebPortal.TransportService.Config;

namespace LunchBag.WebPortal.MessageService
{
    public class MessageService
    {
        private readonly ILogger<MessageService> _logger;
        private readonly INoteRestService _noteRestService;
        private readonly IEventRestService _eventRestService;
        private IAzureServiceBusClient _azureBusManagerClient;
        private IServiceBusClient _serviceBus;
        private readonly MessageServiceOptions _config;
        private string _previousMessageReceived = string.Empty;

        public MessageService(IOptions<MessageServiceOptions> config, IEventRestService eventRestService, INoteRestService noteRestService,
            IAzureServiceBusClient azureBusManagerClient, IServiceBusClient serviceBus, ILogger<MessageService> logger)
        {
            _config = config.Value;
            _logger = logger;
            _eventRestService = eventRestService;
            _noteRestService = noteRestService;
            _azureBusManagerClient = azureBusManagerClient;
            _serviceBus = serviceBus;
        }

        public void Start()
        {
            _azureBusManagerClient.Start(HandleNoteTriggerInput);
        }

        public async Task<bool> HandleNoteTriggerInput(object obj)
        {
            try
            {
                NoteCreatedIoTMessage message = JsonConvert.DeserializeObject<NoteCreatedIoTMessage>(obj.ToString()) as NoteCreatedIoTMessage;
                if (message != null && message.MessageType == IoTMessageType.Note)
                {
                    _logger.LogDebug($"HandleNoteTriggerInput: Retrieved message {JsonConvert.SerializeObject(obj)}");

                    //The previous message sent was exactly the same or has an empty note. Ignoring.
                    if (string.IsNullOrEmpty(message.Note))
                        return true;

                    NoteTemplateModel noteTemplate = await FindBestNoteTemplate(message.EventId, message.Note);
                    if (noteTemplate != null)
                    {
                        if (IsMessageDuplicated(noteTemplate))
                            return true;

                        //Create new note
                        bool result = await CreateNote(message.EventId, noteTemplate.Note, noteTemplate.Sentiment);
                        if (result)
                        {
                            //Push message to Service bus queue
                            await PushNoteToQueue(
                                new NoteCreatedMessage()
                                {
                                    EventId = message.EventId,
                                    Note = noteTemplate.Note
                                }
                            );
                            _logger.LogDebug($"HandleNoteTriggerInput: Note created for event {message.EventId}");
                        }
                        else
                        {
                            _logger.LogError($"HandleNoteTriggerInput: The note could not be created.");
                        }

                        //Update goal value
                        EventModel vUpdatedModel = await UpdateEventSentimentValue(message.EventId, noteTemplate.Note, noteTemplate.Sentiment);
                        if (vUpdatedModel != null)
                        {
                            //Push message to Service bus queue
                            await PushSentimentToQueue(
                                new SentimentsUpdatedMessage()
                                {
                                    EventId = message.EventId,
                                    Sentiments = vUpdatedModel.EventSentiments
                                }
                            );
                            _logger.LogDebug($"HandleNoteTriggerInput: Sentiment updated and pushed to the queue. Value: {vUpdatedModel.EventSentiments} sentiments");
                        }
                        else
                        {
                            _logger.LogDebug($"HandleNoteTriggerInput: The Sentiment for EventId {message.EventId} could not be updated.");
                        }
                    }
                    else
                    {
                        _logger.LogError($"HandleNoteTriggerInput: No NodeTemplate could be found.");
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError($"HandleNoteTriggerInput: {e.Message}");
                return false;
            }
            return true;
        }

        private EventSentimentModel GetEventSentiment(EventModel eventObj, string sentiment)
        {
            EventSentimentModel eventSentiment = eventObj.EventSentiments?.Find(p => p.Name == sentiment);
            if (eventSentiment == null)
            {
                if (eventObj.EventSentiments == null)
                    eventObj.EventSentiments = new List<EventSentimentModel>();
                eventSentiment = new EventSentimentModel()
                {
                    Name = sentiment,
                    Value = 0
                };
                eventObj.EventSentiments.Add(eventSentiment);
            }
            return eventSentiment;
        }

        private void UpdateSentimentValuesPercentage(EventModel eventObj)
        {
            if (eventObj.EventSentiments != null && eventObj.EventSentiments.Count > 0) {
                double total = eventObj.EventSentiments.Sum(p => p.Value);
                foreach (var eventSentiment in eventObj.EventSentiments)
                    eventSentiment.Percentage = total == 0 ? 0 : (100 * eventSentiment.Value) / total;
            }
            return;
        }

        private async Task<bool> CreateNote(string eventId, string note, string sentiment)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);
            if (eventObj != null && eventObj.IsEventActive)
            {
                try
                {
                    NoteModel newNote = new NoteModel()
                    {
                        EventId = eventId,
                        Note = note,
                        Sentiment = sentiment
                    };

                    var result = await _noteRestService.CreateNote(newNote);
                    if (!string.IsNullOrEmpty(result))
                        return true;
                    else
                    {
                        _logger.LogError($"CreateNote: {result}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEventLocationGoalValue: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        private bool IsMessageDuplicated(NoteTemplateModel message)
        {
            string testMessage = $"{message.EventId}_{message.Note}".ToLower();
            if (_previousMessageReceived == testMessage)
                return true;
            _previousMessageReceived = testMessage;
            return false;
        }

        private async Task<NoteTemplateModel> FindBestNoteTemplate(string eventId, string message)
        {
            IEnumerable<NoteTemplateModel> noteTemplates = await _noteRestService.GetNoteTemplates(eventId);
            if (noteTemplates == null)
                return null;

            NoteTemplateModel foundNote = noteTemplates.ToList().Find(p => p.Note.ToLower() == message.ToLower());
            if (foundNote != null)
                return foundNote; 

            Levenshtein distanceTest = new Levenshtein();
            double minimumDistanceValue = -1;
            NoteTemplateModel templateWithMinimumDistance = null;

            foreach (var noteTemplate in noteTemplates) {
                double currentDistance = distanceTest.Distance(message, noteTemplate.Note);
                if(minimumDistanceValue == -1 || minimumDistanceValue > currentDistance)
                {
                    minimumDistanceValue = currentDistance;
                    templateWithMinimumDistance = noteTemplate;
                }
            }

            return templateWithMinimumDistance;
        }

        private async Task<EventModel> UpdateEventSentimentValue(string eventId, string note, string sentiment)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);
            if (eventObj.IsEventActive)
            {
                EventSentimentModel eventSentiment = GetEventSentiment(eventObj, sentiment);
                eventSentiment.Value++;
                eventObj.LastNote = note;

                UpdateSentimentValuesPercentage(eventObj);
                
                try
                {
                    var result = await _eventRestService.UpdateEvent(eventObj);
                    if (result == HttpStatusCode.OK)
                        return eventObj;
                    else if (result == HttpStatusCode.PreconditionFailed)
                        await UpdateEventSentimentValue(eventId, note, sentiment); //Etag error, retrying...
                    else
                    {
                        _logger.LogError($"UpdateEventSentimentValue: {result}");
                        return null;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEventSentimentValue: {e.Message}");
                    return null;
                }
            }
            return null;
        }

        private async Task<bool> PushNoteToQueue(INoteCreatedMessage noteMessage)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(noteMessage, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(noteMessage);
            }
            catch(Exception e)
            {
                _logger.LogError($"PushNoteToQueue: {e.Message}");
                return false;
            }
            return true;
        }

        private async Task<bool> PushSentimentToQueue(ISentimentsUpdatedMessage sentimentMessage)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(sentimentMessage, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(sentimentMessage);
            }
            catch (Exception e)
            {
                _logger.LogError($"PushSentimentToQueue: {e.Message}");
                return false;
            }
            return true;
        }
    }
}
