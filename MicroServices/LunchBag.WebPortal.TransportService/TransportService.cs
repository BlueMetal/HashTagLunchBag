using LunchBag.WebPortal.TransportService.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using LunchBag.Common.Managers;
using RestSharp;
using LunchBag.Common.EventMessages;
using System.Net;
using LunchBag.Common.IoTMessages;
using LunchBag.Common.Models.Transport;

namespace LunchBag.WebPortal.TransportService
{
    public class TransportService
    {
        private static Dictionary<string, DeliveryCacheItem> _deliveryCache = new Dictionary<string, DeliveryCacheItem>();

        private readonly ILogger<TransportService> _logger;
        private readonly ITransportRestService _transportRestService;
        private readonly IAzureServiceBusClient _azureBusManagerClient;
        private readonly IServiceBusClient _serviceBus;
        private readonly TransportServiceOptions _config;

        public TransportService(IOptions<TransportServiceOptions> config,
                                ITransportRestService transportRestService,
                                IAzureServiceBusClient azureBusManagerClient,
                                IServiceBusClient serviceBus,
                                ILogger<TransportService> logger)
        {
            _config = config.Value;
            _logger = logger;
            _transportRestService = transportRestService;
            _azureBusManagerClient = azureBusManagerClient;
            _serviceBus = serviceBus;
        }

        public void Start()
        {
            _azureBusManagerClient.Start(HandleDeliveryTriggerInput);
        }

        private async Task<bool> HandleDeliveryTriggerInput(object obj)
        {
            try
            {
                DeliveryTriggerIoTMessage message = JsonConvert.DeserializeObject<DeliveryTriggerIoTMessage>(obj.ToString())
                    as DeliveryTriggerIoTMessage;
                if (message != null && message.MessageType == IoTMessageType.Unknown && !string.IsNullOrEmpty(message.Parameters))
                {
                    DeliveryIoTMessage update = JsonConvert.DeserializeObject<DeliveryIoTMessage>(message.Parameters);
                    if (update != null)
                    {
                        _logger.LogDebug($"HandleDeliveryTriggerInput: Retrieved message {JsonConvert.SerializeObject(obj)}");

                        // get the target delivery from the transport api
                        DeliveryModel delivery = await GetDelivery(update);
                        if (delivery == null)
                            throw new DeliveryNotFoundException();

                        // update the delivery route with snapped-to-road points
                        await UpdateDeliveryRoute(delivery, update);

                        // update the target delivery with current miles to destination and status 
                        DeliveryModel updatedDelivery = await UpdateDeliveryStatus(delivery, update);

                        // Notify Front End
                        await PushMessageToQueue(
                            new DeliveryUpdatedMessage()
                            {
                                EventId = updatedDelivery.EventId,
                                LocationId = updatedDelivery.LocationId,
                                DeliveryId = updatedDelivery.DeliveryId,
                                RouteId = updatedDelivery.RouteId,
                                Status = updatedDelivery.Status
                            }
                        );

                        _logger.LogDebug($"HandleDeliveryTriggerInput: Delivery updated and pushed to the queue. Status: {updatedDelivery.Status}");
                    }
                }
            }
            catch (DeliveryNotFoundException dnf)
            {
                // this can be a valid situation when vehicles are moving but no deliveries are set up
                _logger.LogInformation($"HandleDeliveryTriggerInput: {dnf.Message}");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"HandleDeliveryTriggerInput: {e.Message}");
                return false;
            }
            return true;
        }

        private async Task<DeliveryModel> GetDelivery(DeliveryIoTMessage message)
        {
            DeliveryModel targetDelivery = null;

            if (message.TripId == "NA")
                return null; // the delivery car is not moving or has not begun a trip

            targetDelivery = await GetCachedDelivery(message.TripId);

            if (targetDelivery == null)
            {
                // find the delivery across all events and cache the result id for better performance
                targetDelivery = await _transportRestService.GetDelivery(message.TripId, message.VehicleId);
                if (targetDelivery != null)
                {
                    if (targetDelivery != null)
                    {
                        _deliveryCache.Add(message.TripId, new DeliveryCacheItem()
                        {
                            deliveryId = targetDelivery.DeliveryId,
                            eventId = targetDelivery.EventId
                        });
                        if (targetDelivery.RouteId != null && targetDelivery.RouteId != message.TripId)
                            _logger.LogInformation($"GetDelivery: Resuming an improperly terminted delivery " +
                                                   $"from {targetDelivery.RouteId} to {message.TripId}.");
                    }
                }
                else
                {
                    _logger.LogError($"GetDelivery: No delivery found");
                }
            }

            if (targetDelivery == null)
                _logger.LogInformation($"GetDelivery: Could not find active delvery for vehicle {message.VehicleId} and trip {message.TripId}");

            return targetDelivery;
        }

        private async Task<DeliveryModel> GetCachedDelivery(string routeId)
        {
            DeliveryCacheItem item;
            bool hasItem = _deliveryCache.TryGetValue(routeId, out item);

            if (!hasItem)
                return null;

            return await _transportRestService.GetDeliveryPerEventId(item.eventId, item.deliveryId);
        }

        private async Task<DeliveryModel> UpdateDeliveryStatus(DeliveryModel delivery, DeliveryIoTMessage update)
        {
            var distanceToDestination = CalculateDistance(Double.Parse(update.Latitude), 
                                                          Double.Parse(update.Longitude), 
                                                          Decimal.ToDouble(delivery.DestinationLocation.latitude), 
                                                          Decimal.ToDouble(delivery.DestinationLocation.longitude));

            var threshold = _config.DistanceToDestinationMilesThreshold;
            var state = distanceToDestination < threshold ? DeliveryModel.kDeliveryStatusComplete 
                                                            : DeliveryModel.kDeliveryStatusInProgress;

            delivery.Status = state;
            delivery.MilesToDestination = (decimal)distanceToDestination;
            delivery.MilesTraveled = update.Distance;
            delivery.RouteId = update.TripId;

            var putResult = await _transportRestService.UpdateDeliveryStatus(delivery);
            if (putResult == HttpStatusCode.OK)
                _logger.LogDebug($"UpdateDeliveryStatus: Delivery {delivery.DeliveryId} " +
                                 $"updated successfully with status {delivery.Status}");
            else
                _logger.LogError($"UpdateDeliveryStatus: {putResult}");
            
            return delivery;
        }

        private async Task UpdateDeliveryRoute(DeliveryModel delivery, DeliveryIoTMessage update)
        {
            RestRequest request = new RestRequest($"Transport/{delivery.EventId}/routes/{delivery.VehicleId}/{update.TripId}" +
                                                  $"?latitude={update.Latitude}&longitude={update.Longitude}", Method.PUT);

            var putResult = await _transportRestService.UpdateDeliveryRoute(delivery.EventId, delivery.VehicleId, update.TripId,
                update.Latitude, update.Longitude);
            if (putResult != HttpStatusCode.OK)
                _logger.LogError($"UpdateDeliveryRoute: {putResult}");
        }

        private async Task<bool> PushMessageToQueue(IDeliveryUpdatedMessage deliveryMessage)
        {
            try
            {
                if (_config.PushToQueueTimeToLive > 0)
                    await _serviceBus.BusAccess.Publish(deliveryMessage,
                        p => { p.TimeToLive = TimeSpan.FromSeconds(_config.PushToQueueTimeToLive); });
                else
                    await _serviceBus.BusAccess.Publish(deliveryMessage);
            }
            catch (Exception e)
            {
                _logger.LogError($"PushMessageToQueue: {e.Message}");
                return false;
            }
            return true;
        }

        // Bing API could be used to calculate distance, but it assumes a route.  Easiest way to compute the current
        // distance to target is raw math based on lat/long to get as-the-crow-flies distance because we don't know
        // the route the driver will take.
        //
        // https://www.movable-type.co.uk/scripts/latlong.html
        private double CalculateDistance(double lat1, double long1, double lat2, double long2)
        {
            const double degreesToRadians = (Math.PI / 180.0);
            const double earthRadius = 6371; // kilometers

            // convert latitude and longitude values to radians
            var radLat1 = lat1 * degreesToRadians;
            var radLong1 = long1 * degreesToRadians;
            var radLat2 = lat2 * degreesToRadians;
            var radLong2 = long2 * degreesToRadians;

            // calculate radian delta between each position.
            var radDeltaLat = radLat2 - radLat1;
            var radDeltaLong = radLong2 - radLong1;

            // calculate distance
            var expr1 = (Math.Sin(radDeltaLat / 2.0) *
                         Math.Sin(radDeltaLat / 2.0)) +
                        (Math.Cos(radLat1) *
                         Math.Cos(radLat2) *
                         Math.Sin(radDeltaLong / 2.0) *
                         Math.Sin(radDeltaLong / 2.0));

            var expr2 = 2.0 * Math.Atan2(Math.Sqrt(expr1),
                                         Math.Sqrt(1 - expr1));

            var distanceInKilometers = (earthRadius * expr2);
            var distanceInMiles = distanceInKilometers * 0.62137;

            return distanceInMiles;
        }


        private class DeliveryCacheItem
        {
            public string deliveryId { get; set; }
            public string eventId { get; set; }
        }


        private class DeliveryNotFoundException : Exception
        {
            public DeliveryNotFoundException() : base("Target delivery not found for vehicle position update")
            { }
        }
    }
}
