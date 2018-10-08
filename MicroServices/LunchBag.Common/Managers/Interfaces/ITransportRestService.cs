using LunchBag.Common.Config;
using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public interface ITransportRestService
    {
        Task<DeliveryModel> GetDelivery(string routeId, string vehicleId);
        Task<DeliveryModel> GetDeliveryPerEventId(string eventId, string deliveryId);
        Task<HttpStatusCode> UpdateDeliveryStatus(DeliveryModel delivery);
        Task<HttpStatusCode> UpdateDeliveryRoute(string eventId, string vehicleId, string tripId, string latitude, string longitude);
        Task<DeliveriesModel> GetDeliveries(string eventId);
        Task<FleetModel> GetFleet(string eventId);
        Task<string> CreateDelivery(DeliveryModel delivery);
        Task<bool> DeleteDelivery(string eventId, string deliveryId);
    }
}
