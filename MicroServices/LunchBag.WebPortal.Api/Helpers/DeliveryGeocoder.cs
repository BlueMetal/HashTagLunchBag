using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LunchBag.Common.IoTMessages;
using LunchBag.Common.Models.Transport;
using LunchBag.WebPortal.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LunchBag.WebPortal.Api.Helpers
{
    public class DeliveryGeocoder
    {
        private readonly ILogger<DeliveryGeocoder> _logger;
        private readonly DeliveryDataManager _deliveryDataManager;
        private readonly RouteDataManager _routeDataManager;

        public DeliveryGeocoder(DeliveryDataManager deliveryDataManager,
                                RouteDataManager routeDataManager,
                                ILogger<DeliveryGeocoder> logger)
        {
            _logger = logger;
            _deliveryDataManager = deliveryDataManager;
            _routeDataManager = routeDataManager;
        }

        public async Task<DeliveryModel> GeocodeDelivery(string eventId, DeliveryModel delivery, bool isUpdate)
        {
            bool skipGeocode = false;

            try
            {
                if (isUpdate)
                {
                    DeliveryModel currentModel = await _deliveryDataManager.GetDeliveryForEvent(eventId, delivery.DeliveryId);
                    if (currentModel != null && currentModel.Destination.Equals(delivery.Destination))
                    {
                        skipGeocode = true;

                        delivery.DestinationLocation = currentModel.DestinationLocation;
                        delivery.DestinationGeocode = currentModel.DestinationGeocode;
                    }
                }

                if (skipGeocode)
                    return delivery;

                GeocodeModel geo = await _routeDataManager.CalculateRouteDestination(delivery.Destination);
                delivery.DestinationGeocode = geo;
                delivery.DestinationLocation = new LocationModel()
                {
                    latitude = geo.location.latitude,
                    longitude = geo.location.longitude
                };

                if (geo.confidence.Equals("low", StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Geocode confidence too low.  Address is not valid for delivery.");

                return delivery;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GeocodeDelivery: {ex.Message}");
                throw ex;
            }
        }
    }
}
