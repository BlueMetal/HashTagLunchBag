using LunchBag.Common.Config;
using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public class TransportRestService : RestServiceBase, ITransportRestService
    {
        public TransportRestService(IOptions<AzureAdOptions> azureConfig, IOptions<RestServiceOptions> config, ILogger<TransportRestService> logger)
            : base(azureConfig, logger, config.Value.RestServiceUrl)
        {
        }

        public async Task<DeliveryModel> GetDelivery(string routeId, string vehicleId)
        {
            RestRequest request = await PrepareQuery("Transport/deliveries/{routeId}/{vehicleId}", Method.GET);
            request.AddUrlSegment("routeId", routeId);
            request.AddUrlSegment("vehicleId", vehicleId);
            var queryResult = await _client.ExecuteTaskAsync<DeliveryModel>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                _logger.LogError($"GetDelivery: {queryResult.ErrorMessage}");
            return null;
        }

        public async Task<DeliveryModel> GetDeliveryPerEventId(string eventId, string deliveryId)
        {
            RestRequest request = await PrepareQuery("Transport/{eventId}/deliveries/{deliveryId}", Method.GET);
            request.AddUrlSegment("eventId", eventId);
            request.AddUrlSegment("deliveryId", deliveryId);

            var queryResult = await _client.ExecuteTaskAsync<DeliveryModel>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                _logger.LogError($"GetDeliveryPerEventId: {queryResult.ErrorMessage}");
            return null;
        }

        public async Task<DeliveriesModel> GetDeliveries(string eventId)
        {
            RestRequest request = await PrepareQuery("Transport/{eventId}/deliveries", Method.GET);
            request.AddUrlSegment("eventId", eventId);

            var queryResult = await _client.ExecuteTaskAsync<DeliveriesModel>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                _logger.LogError($"GetDeliveriesPerEvent: {queryResult.ErrorMessage}");
            return null;
        }

        public async Task<FleetModel> GetFleet(string eventId)
        {
            RestRequest request = await PrepareQuery("Transport/{eventId}/fleet", Method.GET);
            request.AddUrlSegment("eventId", eventId);

            var queryResult = await _client.ExecuteTaskAsync<FleetModel>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                _logger.LogError($"GetFleetPerEvent: {queryResult.ErrorMessage}");
            return null;
        }

        public async Task<string> CreateDelivery(DeliveryModel delivery)
        {
            RestRequest request = await PrepareQuery("Transport/{eventId}/deliveries", Method.POST);
            request.AddUrlSegment("eventId", delivery.EventId);
            request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(delivery),
                                 ParameterType.RequestBody);

            var putResult = await _client.ExecuteTaskAsync<DeliveryModel>(request);
            if (!putResult.IsSuccessful)
                _logger.LogError($"CreateDelivery: Error while sending a message: {putResult.StatusCode}");
            return putResult.Data.Id;
        }

        public async Task<HttpStatusCode> UpdateDeliveryStatus(DeliveryModel delivery)
        {
            RestRequest request = await PrepareQuery("Transport/{eventId}/deliveries/{deliveryId}", Method.PUT);
            request.AddUrlSegment("eventId", delivery.EventId);
            request.AddUrlSegment("deliveryId", delivery.DeliveryId);
            request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(delivery),
                                 ParameterType.RequestBody);

            var putResult = await _client.ExecuteTaskAsync(request);
            if (!putResult.IsSuccessful)
                _logger.LogError($"UpdateDeliveryStatus: Error while sending a message: {putResult.StatusCode}");
            return putResult.StatusCode;
        }

        public async Task<HttpStatusCode> UpdateDeliveryRoute(string eventId, string vehicleId, string tripId, string latitude, string longitude)
        {
            RestRequest request = await PrepareQuery($"Transport/{eventId}/routes/{vehicleId}/{tripId}" +
                                                  $"?latitude={latitude}&longitude={longitude}", Method.PUT);

            var putResult = await _client.ExecuteTaskAsync(request);
            if (!putResult.IsSuccessful)
                _logger.LogError($"UpdateDeliveryRoute: Error while sending a message: {putResult.StatusCode}");
            return putResult.StatusCode;
        }

        public async Task<bool> DeleteDelivery(string eventId, string deliveryId)
        {
            RestRequest request = await PrepareQuery("Transport/{eventId}/deliveries/{deliveryId}", Method.DELETE);
            request.AddUrlSegment("eventId", eventId);
            request.AddUrlSegment("deliveryId", deliveryId);
            var result = await _client.ExecuteTaskAsync<bool>(request);
            if (!result.IsSuccessful)
                _logger.LogError($"DeleteDelivery: Error while deleting delivery {deliveryId} related to eventId {eventId}: {result.StatusCode}");
            return result.Data;
        }
    }
}
