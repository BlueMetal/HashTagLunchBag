using LunchBag.Common.Config;
using LunchBag.Common.EventMessages;
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
    public class SignalsRestService : RestServiceBase, ISignalsRestService
    {
        public SignalsRestService(IOptions<AzureAdOptions> azureConfig, IOptions<RestServiceOptions> config, ILogger<SignalsRestService> logger)
            : base(azureConfig, logger, config.Value.RestServiceUrl)
        {
        }

        public async Task<HttpStatusCode> GoalUpdated(IGoalUpdatedMessage message)
        {
            RestRequest request = await PrepareQuery("Signals/Goal", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(message), ParameterType.RequestBody);

            var queryResult = await _client.ExecuteTaskAsync(request);
            if(!queryResult.IsSuccessful)
                _logger.LogError($"GoalUpdated: Error while sending a message: {queryResult.StatusCode}");
            return queryResult.StatusCode;
        }

        public async Task<HttpStatusCode> NoteCreated(INoteCreatedMessage message)
        {
            RestRequest request = await PrepareQuery("Signals/Note", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(message), ParameterType.RequestBody);

            var queryResult = await _client.ExecuteTaskAsync(request);
            if (!queryResult.IsSuccessful)
                _logger.LogError($"NoteCreated: Error while sending a message: {queryResult.StatusCode}");
            return queryResult.StatusCode;
        }

        public async Task<HttpStatusCode> SentimentUpdated(ISentimentsUpdatedMessage message)
        {
            RestRequest request = await PrepareQuery("Signals/Sentiment", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(message), ParameterType.RequestBody);

            var queryResult = await _client.ExecuteTaskAsync(request);
            if (!queryResult.IsSuccessful)
                _logger.LogError($"SentimentUpdated: Error while sending a message: {queryResult.StatusCode}");
            return queryResult.StatusCode;
        }

        public async Task<HttpStatusCode> ViewUpdatedUpdated(IViewUpdatedMessage message)
        {
            RestRequest request = await PrepareQuery("Signals/View", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(message), ParameterType.RequestBody);

            var queryResult = await _client.ExecuteTaskAsync(request);
            if (!queryResult.IsSuccessful)
                _logger.LogError($"ViewUpdatedUpdated: Error while sending a message: {queryResult.StatusCode}");
            return queryResult.StatusCode;
        }

        public async Task<HttpStatusCode> EventActiveStateChanged(IEventActiveStateChangedMessage message)
        {
            RestRequest request = await PrepareQuery("Signals/ActiveState", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(message), ParameterType.RequestBody);

            var queryResult = await _client.ExecuteTaskAsync(request);
            if (!queryResult.IsSuccessful)
                _logger.LogError($"EventActiveStateChanged: Error while sending a message: {queryResult.StatusCode}");
            return queryResult.StatusCode;
        }

        public async Task<HttpStatusCode> DeliveryUpdated(IDeliveryUpdatedMessage message)
        {
            RestRequest request = await PrepareQuery("Signals/Deliveries", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(message), ParameterType.RequestBody);

            var queryResult = await _client.ExecuteTaskAsync(request);
            if (!queryResult.IsSuccessful)
                _logger.LogError($"DeliveryUpdated: Error while sending a message: {queryResult.StatusCode}");
            return queryResult.StatusCode;
        }
    }
}
