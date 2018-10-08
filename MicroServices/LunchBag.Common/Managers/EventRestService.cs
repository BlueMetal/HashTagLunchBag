using LunchBag.Common.Config;
using LunchBag.Common.Models;
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
    public class EventRestService : RestServiceBase, IEventRestService
    {
        public EventRestService(IOptions<AzureAdOptions> azureConfig, IOptions<RestServiceOptions> config, ILogger<EventRestService> logger)
            : base(azureConfig, logger, config.Value.RestServiceUrl)
        {
        }

        public async Task<EventModel> GetEvent(string eventId)
        {
            RestRequest request = await PrepareQuery("Events/{eventId}", Method.GET);
            request.AddUrlSegment("eventId", eventId);
            var queryResult = await _client.ExecuteTaskAsync<EventModel>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                _logger.LogError($"GetEvent: {queryResult.ErrorMessage}");
            return null;
        }

        public async Task<HttpStatusCode> UpdateEvent(EventModel eventObj)
        {
            RestRequest request = await PrepareQuery("Events", Method.PUT);
            request.AddParameter("application/json", JsonConvert.SerializeObject(eventObj), ParameterType.RequestBody);
            var queryResult = await _client.ExecuteTaskAsync<EventModel>(request);
            if (!queryResult.IsSuccessful)
                _logger.LogError($"UpdateEvent: Error while sending a message: {queryResult.StatusCode}");
            return queryResult.StatusCode;
        }

        public async Task<IEnumerable<EventModel>> GetEvents()
        {
            RestRequest request = await PrepareQuery("Events", Method.GET);
            var queryResult = await _client.ExecuteTaskAsync<IEnumerable<EventModel>>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                _logger.LogError($"GetEvents: {queryResult.ErrorMessage}");
            return null;
        }

        public async Task<string> CreateEvent(EventModel eventObj)
        {
            RestRequest request = await PrepareQuery("Events", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(eventObj), ParameterType.RequestBody);
            var result = await _client.ExecuteTaskAsync<string>(request);
            if (!result.IsSuccessful)
                _logger.LogError($"CreateEvent: Error while creating an event: {result.StatusCode}");
            return result.Data;
        }

        public async Task<bool> DeleteEvent(string eventId)
        {
            RestRequest request = await PrepareQuery("Events/{eventId}", Method.DELETE);
            request.AddUrlSegment("eventId", eventId);
            var result = await _client.ExecuteTaskAsync<bool>(request);
            if (!result.IsSuccessful)
                _logger.LogError($"DeleteEvent: Error while deleting an event: {result.StatusCode}");
            return result.Data;
        }
    }
}
