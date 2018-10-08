using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using LunchBag.WebPortal.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.Api.Helpers
{
    public class DeliveryDataManager
    {
        private readonly WebPortalApiConfig _config;
        private readonly Dictionary<string, IEnumerable<DeliveryModel>> _dictDeliveries;
        private readonly ICosmosDBRepository<DeliveryModel> _repoDeliveries;
        private readonly ILogger<DeliveryDataManager> _logger;

        public DeliveryDataManager(IOptions<WebPortalApiConfig> config,
                                   ICosmosDBRepository<DeliveryModel> repoDeliveries,
                                   ILogger<DeliveryDataManager> logger)
        {
            _logger = logger;
            _config = config.Value;
            _repoDeliveries = repoDeliveries;
            _dictDeliveries = new Dictionary<string, IEnumerable<DeliveryModel>>();
        }

        public async Task<IEnumerable<DeliveryModel>> GetDeliveriesForEvent(string eventId, bool forceRefresh = false)
        {
            IEnumerable<DeliveryModel> deliveries = null;
            if (!_dictDeliveries.ContainsKey(eventId) || !_config.UseCache || (_config.UseCache && forceRefresh))
            {
                deliveries = await _repoDeliveries.GetItemsAsyncOrderByDescending(p => p.EventId == eventId,
                                                                                  p => p.Date);
                if (deliveries == null)
                    _logger.LogDebug($"GetDeliveriesForEvent: No deliveries found for event {eventId}.");
                _dictDeliveries[eventId] = deliveries;
            }
            else
            {
                deliveries = _dictDeliveries[eventId];
            }
            return deliveries;
        }

        public async Task<DeliveryModel> GetDeliveryForEvent(string eventId, string deliveryId, bool forceRefresh = false)
        {
            if (!_dictDeliveries.ContainsKey(eventId) || !_config.UseCache || (_config.UseCache && forceRefresh))
                await GetDeliveriesForEvent(eventId, forceRefresh);

            return _dictDeliveries[eventId].FirstOrDefault(p => p.DeliveryId == deliveryId 
                                                                && p.EventId == eventId);
        }

        public async Task<DeliveryModel> CreateDeliveryForEvent(string eventId, DeliveryModel delivery)
        {
            await _repoDeliveries.CreateItemAsync(delivery);
            if (_config.UseCache)
                await GetDeliveriesForEvent(eventId, true);

            return await GetDeliveryForEvent(eventId, delivery.DeliveryId);
        }

        public async Task<DeliveryModel> UpdateDeliveryForEvent(string eventId, DeliveryModel delivery)
        {
            await _repoDeliveries.UpdateItemAsync(delivery.Id, delivery);
            if (_config.UseCache)
                await GetDeliveryForEvent(eventId, delivery.DeliveryId, true);

            return await GetDeliveryForEvent(eventId, delivery.DeliveryId);
        }

        public async Task<bool> DeleteDeliveryForEvent(string eventId, string deliveryId)
        {
            bool result = await _repoDeliveries.DeleteItemsAsync(p => p.DeliveryId == deliveryId
                                                                 && p.EventId == eventId);
            if (result)
                _dictDeliveries?.Remove(eventId); // event deliveries will be reloaded on next 'get'
            return result;
        }

        public async Task<DeliveryModel> FindDelivery(string routeId, string vehicleId)
        {
            // routes and vehicles are unique to events, so we can find by either of these values without risk
            //  of event crossover
            IEnumerable<DeliveryModel> availableDeliveries
                = await _repoDeliveries.GetItemsAsyncOrderByDescending(p => p.RouteId == routeId
                                                                            && p.VehicleId == vehicleId
                                                                            && (p.Status == DeliveryModel.kDeliveryStatusNotStarted
                                                                                || p.Status == DeliveryModel.kDeliveryStatusInProgress),
                                                                       p => p.Date);
            DeliveryModel delivery = availableDeliveries.FirstOrDefault();

            // if there are no deliveries for the route, look at the most recent delivery for the vehicle
            if (delivery == null)
            {
                availableDeliveries = await _repoDeliveries.GetItemsAsyncOrderByDescending(p => p.VehicleId == vehicleId,
                                                                                           p => p.Date);

                DeliveryModel candidate = availableDeliveries.FirstOrDefault();
                if (candidate != null && candidate.Status == DeliveryModel.kDeliveryStatusNotStarted)
                {
                    // typical scenario of a delivery that hasn't yet started - this is our target
                    delivery = candidate;
                }
                else if (candidate != null && candidate.Status == DeliveryModel.kDeliveryStatusInProgress)
                {
                    // assume delivery is resuming if the most recent delivery for the vehicle never ended
                    delivery = candidate;
                }
            }

            return delivery;
        }
    }
}
