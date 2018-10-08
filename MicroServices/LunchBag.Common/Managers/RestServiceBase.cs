using LunchBag.Common.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public class RestServiceBase
    {
        private readonly string _clientId;
        private readonly AuthenticationContext _authContext = null;
        private readonly ClientCredential _clientCredential = null;
        protected readonly ILogger<RestServiceBase> _logger;
        protected readonly RestClient _client;

        public RestServiceBase(IOptions<AzureAdOptions> azureConfig, ILogger<RestServiceBase> logger, string restServiceUrl)
        {
            _logger = logger;
            _clientId = azureConfig.Value.ClientId;
            _authContext = new AuthenticationContext(azureConfig.Value.Instance + azureConfig.Value.TenantId);
            _clientCredential = new ClientCredential(azureConfig.Value.ClientId, azureConfig.Value.ClientSecret);
            _client = new RestClient(restServiceUrl);
        }

        private async Task<string> GetAzureAdToken()
        {
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = await _authContext.AcquireTokenAsync(_clientId, _clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }
                    _logger.LogError($"An error occurred while acquiring a token: {ex.Message}. Retry: {retry.ToString()}");
                }

            } while ((retry == true) && (retryCount < 3));

            return result?.AccessToken;
        }

        protected async Task<RestRequest> PrepareQuery(string endpoint, Method method)
        {
            var token = await GetAzureAdToken();

            RestRequest request = new RestRequest(endpoint, method);
            request.AddHeader("Authorization", $"Bearer {token}");

            return request;
        }
    }
}
