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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LunchBag.WebPortal.Api
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class TransportController : ControllerBase
    {
        private readonly DeliveryDataManager _deliveryDataManager;
        private readonly RouteDataManager _routeDataManager;
        private readonly MobiliyaDataManager _mobiliyaDataManager;
        private readonly DeliveryGeocoder _deliveryGeocoder;

        public TransportController(DeliveryDataManager deliveryDataManager,
                                   RouteDataManager routeDataManager,
                                   MobiliyaDataManager mobiliyaDataManager,
                                   DeliveryGeocoder deliveryGeocoder)
        {
            _deliveryDataManager = deliveryDataManager;
            _routeDataManager = routeDataManager;
            _mobiliyaDataManager = mobiliyaDataManager;
            _deliveryGeocoder = deliveryGeocoder;
        }

        [HttpGet("{eventId}/deliveries")]
        [AllowAnonymous]
        [Produces(typeof(DeliveriesModel))]
        public async Task<IActionResult> GetDeliveries(string eventId)
        {
            DeliveriesModel deliveries = new DeliveriesModel();
            deliveries.Deliveries = await _deliveryDataManager.GetDeliveriesForEvent(eventId);
            foreach (DeliveryModel delivery in deliveries.Deliveries)
            {
                deliveries.Count++;
                deliveries.TotalLunches += delivery.LunchCount;
                deliveries.TotalMiles += (int)delivery.MilesTraveled;
            }
            return Ok(deliveries);
        }
        
        [HttpGet("{eventId}/deliveries/{deliveryId}")]
        [Produces(typeof(DeliveryModel))]
        public async Task<IActionResult> GetDelivery(string eventId, string deliveryId)
        {
            DeliveryModel delivery = await _deliveryDataManager.GetDeliveryForEvent(eventId, deliveryId);
            return Ok(delivery);
        }

        [HttpPost("{eventId}/deliveries")]
        [Produces(typeof(DeliveryModel))]
        public async Task<IActionResult> CreateDelivery(string eventId, [FromBody]DeliveryModel delivery)
        {
            if (delivery.DeliveryId == null || delivery.DeliveryId.Length == 0)
                delivery.DeliveryId = Guid.NewGuid().ToString();

            delivery = await _deliveryGeocoder.GeocodeDelivery(eventId, delivery, false);

            DeliveryModel createdDelivery 
                = await _deliveryDataManager.CreateDeliveryForEvent(eventId, delivery);
            return Ok(createdDelivery);
        }

        [HttpPut("{eventId}/deliveries/{deliveryId}")]
        [Produces(typeof(DeliveryModel))]
        public async Task<IActionResult> UpdateDelivery(string eventId, string deliveryId, [FromBody]DeliveryModel delivery)
        {
            if (!string.Equals(deliveryId, delivery.DeliveryId))
                throw new Exception("DeliveryId mismatch - make sure you are updating the appropriate entity");

            delivery = await _deliveryGeocoder.GeocodeDelivery(eventId, delivery, true);

            DeliveryModel updatedDelivery 
                = await _deliveryDataManager.UpdateDeliveryForEvent(eventId, delivery);
            return Ok(updatedDelivery);
        }

        [HttpDelete("{eventId}/deliveries/{deliveryId}")]
        public async Task<IActionResult> DeleteDelivery(string eventId, string deliveryId)
        {
            var result = await _deliveryDataManager.DeleteDeliveryForEvent(eventId, deliveryId);
            return Ok(result);
        }

        [HttpGet("deliveries/{routeId}/{vehicleId}")]
        [Produces(typeof(DeliveryModel))]
        public async Task<IActionResult> FindDelivery(string routeId, string vehicleId)
        {
            DeliveryModel target = await _deliveryDataManager.FindDelivery(routeId, vehicleId);

            if (target == null)
                return NotFound();

            return Ok(target);
        }

        [HttpGet("{eventId}/routes/{routeId}")]
        [AllowAnonymous]
        [Produces(typeof(RouteModel))]
        public async Task<IActionResult> GetDeliveryRoute(string eventId, string routeId)
        {
            RouteModel route = await _routeDataManager.GetRoute(routeId);
            return Ok(route);
        }

        [HttpPut("{eventId}/routes/{vehicleId}/{routeId}")]
        public async Task<IActionResult> UpdateDeliveryRoute(string eventId, string vehicleId, string routeId, 
                                                             [FromQuery]string latitude, [FromQuery]string longitude)
        {
            await _routeDataManager.UpdateRoute(vehicleId, routeId, latitude, longitude);
            return Ok();
        }

        [HttpGet("{eventId}/fleet")]
        [Produces(typeof(FleetModel))]
        public async Task<IActionResult> GetEventFleet(string eventId)
        {
            FleetModel fleet = await _mobiliyaDataManager.GetEventVehicles(eventId);
            return Ok(fleet);
        }
    }
}
