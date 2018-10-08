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
    public class RegistrationRestService : RestServiceBase, IRegistrationRestService
    {
        public RegistrationRestService(IOptions<AzureAdOptions> azureConfig, IOptions<RestRegistrationServiceOptions> config, ILogger<RegistrationRestService> logger)
            : base(azureConfig, logger, config.Value.RestServiceUrl)
        {
        }

        public async Task<byte[]> GetRegistrationsExtract()
        {
            RestRequest request = await PrepareQuery("Registrations/extract", Method.GET);
            request.RequestFormat = DataFormat.Json;
            var queryResult = await _client.ExecuteTaskAsync(request);
            if (queryResult.IsSuccessful)
            {
                var bytes = JsonConvert.DeserializeObject<byte[]>(queryResult.Content);
                return bytes;
            }  
            else
                _logger.LogError($"GetRegistrationsExtract: {queryResult.ErrorMessage}");
            return null;
        }
    }
}
