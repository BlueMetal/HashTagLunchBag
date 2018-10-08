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
using LunchBag.AdminPortal.Config;
using LunchBag.Common.Models.Transport;

namespace LunchBag.AdminPortal
{
    public class AdminService
    {
        private readonly ILogger<AdminService> _logger;
        private readonly IEventRestService _eventRestService;
        private readonly INoteRestService _noteRestService;
        private readonly ITransportRestService _transportRestService;
        private readonly IServiceBusClient _serviceBus;
        private readonly AdminServiceOptions _config;

        public AdminService(IOptions<AdminServiceOptions> config, IEventRestService eventRestService, INoteRestService noteRestService,
            ITransportRestService transportRestService, IServiceBusClient serviceBus, ILogger<AdminService> logger)
        {
            _config = config.Value;
            _logger = logger;
            _eventRestService = eventRestService;
            _noteRestService = noteRestService;
            _transportRestService = transportRestService;
            _serviceBus = serviceBus;
        }

        public async Task<string> AddEvent(EventModel eventObj)
        {
            if (eventObj != null)
                return await _eventRestService.CreateEvent(eventObj);
            return string.Empty;
        }

        public async Task<EventModel> GetEvent(string eventId)
        {
            return await _eventRestService.GetEvent(eventId);
        }

        public async Task<bool> DeleteEvent(string eventId)
        {
            return await _eventRestService.DeleteEvent(eventId);
        }

        public async Task<IEnumerable<EventModel>> GetEvents()
        {
            return await _eventRestService.GetEvents();
        }

        public async Task<IEnumerable<NoteTemplateModel>> GetNoteTemplates(string eventId)
        {
            return await _noteRestService.GetNoteTemplates(eventId);
        }

        public async Task<DeliveriesModel> GetDeliveries(string eventId)
        {
            return await _transportRestService.GetDeliveries(eventId);
        }

        public async Task<FleetModel> GetFleet(string eventId)
        {
            return await _transportRestService.GetFleet(eventId);
        }

        public async Task<bool> UpdateEventGeneralSettings(string eventId, string eventName, bool isEventActive)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);

            if (eventObj != null)
            {
                bool needUpdateActiveStatus = eventObj.IsEventActive != isEventActive;

                //Update parameters
                eventObj.EventName = eventName;
                eventObj.IsEventActive = isEventActive;

                try
                {
                    var result = await _eventRestService.UpdateEvent(eventObj);
                    if (result == HttpStatusCode.OK)
                    {
                        if (needUpdateActiveStatus)
                        {
                            if (!await PushMessageToQueue(new EventActiveStateChangedMessage() { EventId = eventObj.Id, EventActiveState = eventObj.IsEventActive }))
                                _logger.LogError($"PushMessages: Error while pushing to queue for Event Active State.");
                        }
                        return true;
                    }
                    else if (result == HttpStatusCode.PreconditionFailed)
                        return await UpdateEventGeneralSettings(eventId, eventName, isEventActive); //Etag error, retrying...
                    else
                    {
                        _logger.LogError($"UpdateEvent: {result}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEvent: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> UpdateEventLastNoteSettings(string eventId, string lastNote)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);

            if (eventObj != null)
            {
                bool needCreateNote = eventObj.LastNote != lastNote;
                if (!needCreateNote)
                    return true;

                //Update parameters
                eventObj.LastNote = lastNote;

                try
                {
                    var result = await _eventRestService.UpdateEvent(eventObj);
                    if (result == HttpStatusCode.OK)
                    {
                        if (needCreateNote)
                        {
                            if (!await PushMessageToQueue(new NoteCreatedMessage() { EventId = eventObj.Id, Note = lastNote }))
                                _logger.LogError($"PushMessages: Error while pushing to queue for Last Note.");
                        }
                        return true;
                    }
                    else if (result == HttpStatusCode.PreconditionFailed)
                        return await UpdateEventLastNoteSettings(eventId, lastNote); //Etag error, retrying...
                    else
                    {
                        _logger.LogError($"UpdateEvent: {result}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEvent: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> UpdateEventPaypalSettings(string eventId, string clientId, string secret,
            string email, string businessName, string donationName, string currency, string thanksnote)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);

            if (eventObj != null)
            {
                //Update parameters
                if (eventObj.PayPalApi == null)
                    eventObj.PayPalApi = new EventPayPalApiModel();

                eventObj.PayPalApi.ClientId = clientId;
                eventObj.PayPalApi.Secret = secret;

                if (eventObj.PayPalApi.MerchantInfo == null)
                    eventObj.PayPalApi.MerchantInfo = new PayPalMerchantInfo();

                eventObj.PayPalApi.MerchantInfo.Email = email;
                eventObj.PayPalApi.MerchantInfo.BusinessName = businessName;
                eventObj.PayPalApi.MerchantInfo.DonationName = donationName;
                eventObj.PayPalApi.MerchantInfo.Currency = currency;
                eventObj.PayPalApi.MerchantInfo.ThanksNote = thanksnote;

                try
                {
                    var result = await _eventRestService.UpdateEvent(eventObj);
                    if (result == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else if (result == HttpStatusCode.PreconditionFailed)
                        return await UpdateEventPaypalSettings(eventId, clientId, secret, email, businessName, donationName, currency, thanksnote); //Etag error, retrying...
                    else
                    {
                        _logger.LogError($"UpdateEventPaypalSettings: {result}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEventPaypalSettings: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> UpdateEventLocations(string eventId, List<EventLocationModel> eventLocations)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);

            if (eventObj != null)
            {
                eventObj.EventLocations = eventLocations;

                try
                {
                    var result = await _eventRestService.UpdateEvent(eventObj);
                    if (result == HttpStatusCode.OK)
                    {
                        foreach (var location in eventObj.EventLocations)
                        {
                            if (!await PushMessageToQueue(new GoalUpdatedMessage() { EventId = eventObj.Id, LocationId = location.Id, GoalStatus = location.GoalStatus }))
                                _logger.LogError($"PushMessages: Error while pushing to queue for Event Location '{location.Id}'.");
                        }
                        return true;
                    }
                    else if (result == HttpStatusCode.PreconditionFailed)
                        return await UpdateEventLocations(eventId, eventLocations); //Etag error, retrying...
                    else
                    {
                        _logger.LogError($"UpdateEventLocations: {result}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEventLocations: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> UpdateEventSentiments(string eventId, List<EventSentimentModel> eventSentiments)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);

            if (eventObj != null)
            {
                eventObj.EventSentiments = eventSentiments;
                UpdateSentimentValuesPercentage(eventObj);

                try
                {
                    var result = await _eventRestService.UpdateEvent(eventObj);
                    if (result == HttpStatusCode.OK)
                    {
                        if (eventObj.EventSentiments != null)
                        {
                            if (!await PushMessageToQueue(new SentimentsUpdatedMessage() { EventId = eventObj.Id, Sentiments = eventObj.EventSentiments }))
                                _logger.LogError($"PushMessages: Error while pushing to queue for Event Sentiment '{eventObj.EventSentiments.Count}' sentiments.");
                        }
                        return true;
                    }
                    else if (result == HttpStatusCode.PreconditionFailed)
                        return await UpdateEventSentiments(eventId, eventSentiments); //Etag error, retrying...
                    else
                    {
                        _logger.LogError($"UpdateEventSentiments: {result}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEventSentiments: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> UpdateEventViews(string eventId, List<EventViewModel> eventViews)
        {
            EventModel eventObj = await _eventRestService.GetEvent(eventId);

            if (eventObj != null)
            {
                eventObj.EventViews = eventViews;

                try
                {
                    var result = await _eventRestService.UpdateEvent(eventObj);
                    if (result == HttpStatusCode.OK)
                    {
                        if (eventObj.EventViews != null)
                        {
                            foreach (var view in eventObj.EventViews)
                            {
                                if (!await PushMessageToQueue(new ViewUpdatedMessage() { EventId = eventObj.Id, ViewId = view.ViewId, Views = view.Views, CyclingInterval = view.CyclingInterval }))
                                    _logger.LogError($"PushMessages: Error while pushing to queue for Event View '{view.ViewId}'.");
                            }
                        }
                        return true;
                    }
                    else if (result == HttpStatusCode.PreconditionFailed)
                        return await UpdateEventViews(eventId, eventViews); //Etag error, retrying...
                    else
                    {
                        _logger.LogError($"UpdateEventViews: {result}");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"UpdateEventViews: {e.Message}");
                    return false;
                }
            }
            return false;
        }

        public async Task<string> AddNoteTemplate(NoteTemplateModel noteTemplate)
        {
            if (noteTemplate != null)
                return await _noteRestService.CreateNoteTemplate(noteTemplate);
            return string.Empty;
        }

        public async Task<bool> DeleteNoteTemplate(string noteTemplateId)
        {
            return await _noteRestService.DeleteNoteTemplate(noteTemplateId);
        }

        public async Task<bool> DeleteDelivery(string eventId, string deliveryId)
        {
            return await _transportRestService.DeleteDelivery(eventId, deliveryId);
        }

        public async Task<bool> DeleteAllNotes(string eventId)
        {
            return await _noteRestService.DeleteAllNotes(eventId);
        }

        public async Task<string> AddDelivery(DeliveryModel delivery)
        {
            FleetModel fleet = await _transportRestService.GetFleet(delivery.EventId);

            if (fleet != null) {
                FleetVehicle vehicule = fleet.Vehicles.FirstOrDefault(p => p.DriverId == delivery.DriverId);
                if(vehicule != null)
                {
                    delivery.DriverName = vehicule.DriverName;
                    delivery.VehicleId = vehicule.VehicleId;
                    delivery.VehicleName = vehicule.VehicleName;
                    return await _transportRestService.CreateDelivery(delivery);
                }
            }
            return string.Empty;
        }

        private void UpdateSentimentValuesPercentage(EventModel eventObj)
        {
            if (eventObj.EventSentiments != null && eventObj.EventSentiments.Count > 0)
            {
                double total = eventObj.EventSentiments.Sum(p => p.Value);
                foreach (var eventSentiment in eventObj.EventSentiments)
                    eventSentiment.Percentage = total == 0 ? 0 : (100 * eventSentiment.Value) / total;
            }
            return;
        }

        private async Task<bool> PushMessageToQueue(IGoalUpdatedMessage message)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(message, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(message);
            }
            catch (Exception e)
            {
                _logger.LogError($"PushMessageToQueue: {e.Message}");
                return false;
            }
            return true;
        }

        private async Task<bool> PushMessageToQueue(ISentimentsUpdatedMessage message)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(message, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(message);
            }
            catch (Exception e)
            {
                _logger.LogError($"PushMessageToQueue: {e.Message}");
                return false;
            }
            return true;
        }

        private async Task<bool> PushMessageToQueue(INoteCreatedMessage message)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(message, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(message);
            }
            catch (Exception e)
            {
                _logger.LogError($"PushMessageToQueue: {e.Message}");
                return false;
            }
            return true;
        }

        private async Task<bool> PushMessageToQueue(IEventActiveStateChangedMessage message)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(message, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(message);
            }
            catch (Exception e)
            {
                _logger.LogError($"PushMessageToQueue: {e.Message}");
                return false;
            }
            return true;
        }

        private async Task<bool> PushMessageToQueue(IViewUpdatedMessage message)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(message, p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(message);
            }
            catch (Exception e)
            {
                _logger.LogError($"PushMessageToQueue: {e.Message}");
                return false;
            }
            return true;
        }
    }
}
