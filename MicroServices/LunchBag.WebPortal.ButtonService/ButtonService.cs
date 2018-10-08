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
using LunchBag.Common.Config;
using LunchBag.WebPortal.TransportService.Config;
using Microsoft.Extensions.Logging.Abstractions;

namespace LunchBag.WebPortal.ButtonService
{
    public class ButtonService
    {
        private readonly ILogger<ButtonService> _logger;
        private readonly IEventRestService _eventRestService;
        private readonly IAzureServiceBusClient _azureBusManagerClient;
        private readonly IServiceBusClient _serviceBus;
        private readonly ButtonServiceOptions _config;

        public ButtonService(IOptions<ButtonServiceOptions> config, IEventRestService eventRestService, 
            IAzureServiceBusClient azureBusManagerClient, IServiceBusClient serviceBus, ILogger<ButtonService> logger)
        {
            _config = config.Value;
            _logger = logger;
            _eventRestService = eventRestService;
            _azureBusManagerClient = azureBusManagerClient;
            _serviceBus = serviceBus;
        }

        public void Start()
        {
            _azureBusManagerClient.Start(HandleButtonTriggerInput);
        }

        public async Task<bool> HandleButtonTriggerInput(object obj)
        {
            try
            {
                ButtonTriggerIoTMessage message = JsonConvert.DeserializeObject<ButtonTriggerIoTMessage>(obj.ToString()) as ButtonTriggerIoTMessage;
                if (message != null && message.MessageType == IoTMessageType.Button)
                {
                    _logger.LogDebug($"HandleButtonTriggerInput: Retrieved message {JsonConvert.SerializeObject(obj)}");

                    //Update goal value
                    int newGoalValue = await UpdateEventLocationGoalValue(message.EventId, message.LocationId, message.Capacity);
                    if (newGoalValue != -1) {
                        //Push message to Service bus queue
                        await PushMessageToQueue(
                            new GoalUpdatedMessage() {
                                EventId = message.EventId,
                                LocationId = message.LocationId,
                                GoalStatus = newGoalValue
                            }
                        );
                        _logger.LogDebug($"HandleButtonTriggerInput: Goal updated and pushed to the queue. Value: {newGoalValue}");
                    }
                    else
                    {
                        _logger.LogDebug($"HandleButtonTriggerInput: The Goal for EventId {message.EventId} and LocationId {message.LocationId} could not be updated.");
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError($"HandleButtonTriggerInput: {e.Message}");
                return false;
            }
            return true;
        }

        private async Task<int> UpdateEventLocationGoalValue(string eventId, string locationId, int value)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);
            if (eventObj != null)
            {
                EventLocationModel eventLocation = eventObj.EventLocations?.Find(p => p.Id == locationId);
                if (eventLocation != null && eventObj.IsEventActive)
                {
                    eventLocation.GoalStatus += value;
                    try
                    {
                        var result = await _eventRestService.UpdateEvent(eventObj);
                        if (result == HttpStatusCode.OK)
                            return eventLocation.GoalStatus;
                        else if (result == HttpStatusCode.PreconditionFailed)
                            await UpdateEventLocationGoalValue(eventId, locationId, value); //Etag error, retrying...
                        else
                        {
                            _logger.LogError($"UpdateEventLocationGoalValue: {result}");
                            return -1;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"UpdateEventLocationGoalValue: {e.Message}");
                        return -1;
                    }
                }
            } else
            {
                _logger.LogError($"The event {eventId} wasn't found.");
            }
            return -1;
        }

        private async Task<bool> PushMessageToQueue(IGoalUpdatedMessage buttonMessage)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(buttonMessage, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(buttonMessage);
            }
            catch(Exception e)
            {
                _logger.LogError($"PushMessageToQueue: {e.Message}");
                return false;
            }
            return true;
        }
    }
}
