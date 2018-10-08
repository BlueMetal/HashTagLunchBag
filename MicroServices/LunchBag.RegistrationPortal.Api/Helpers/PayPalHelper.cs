using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.RegistrationPortal.Api.Config;
using LunchBag.RegistrationPortal.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RestSharp;
using RestSharp.Authenticators;


namespace LunchBag.RegistrationPortal.Api.Helpers
{
    public class PayPalHelper : IDisposable
    {
        private readonly PayPalApiConfig _config;
        private readonly RestClient _paypalClient;
        private readonly ILogger<PayPalHelper> _logger;

        // pay pal config is an event-level config to allow for multiple 
        //  pay pal accounts in the system - one per event
        private readonly ICosmosDBRepository<EventModel> _repoEvents;

        private static string _access_token = "not_granted";

        public PayPalHelper(IOptions<PayPalApiConfig> config,
                            ILogger<PayPalHelper> logger,
                            ICosmosDBRepository<EventModel> repoEvents)
        {
            _logger = logger;
            _config = config.Value;
            _paypalClient = new RestClient(_config.Url);
            _repoEvents = repoEvents;
        }

        public async Task<string> SendInvoiceAsync(string eventId, RegistrationModel recipient, double amount)
        {
            string invoiceRefNumber = null;

            EventModel targetEvent = await _repoEvents.GetItemAsync(eventId);
            if (targetEvent == null)
                throw new Exception("Could not find target event in the events repository.");

            var draftResult = await CallApiAsync(async () =>
            {
                RestRequest request = new RestRequest($"invoicing/invoices", Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddParameter("Authorization", $"Bearer {_access_token}", ParameterType.HttpHeader);

                var requestBod = GenerateInvoiceRequest(targetEvent, recipient, amount);
                request.AddParameter("application/json", JsonConvert.SerializeObject(requestBod), ParameterType.RequestBody);

                return await _paypalClient.ExecuteTaskAsync<string>(request);
            }, 
            targetEvent.PayPalApi.ClientId, targetEvent.PayPalApi.Secret);

            if (draftResult.IsSuccessful)
            {
                var draftResponse = JsonConvert.DeserializeObject<PayPalInvoiceDraftResponse>(draftResult.Data);
                var sendLink = draftResponse.links.First(p => p.rel.Equals("send", StringComparison.InvariantCultureIgnoreCase));
                var sendResult = await CallApiAsync(async () =>
                {
                    RestRequest request = new RestRequest(sendLink.href, PayPalLink2Method(sendLink));
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("Authorization", $"Bearer {_access_token}", ParameterType.HttpHeader);

                    return await _paypalClient.ExecuteTaskAsync<string>(request);
                },
                targetEvent.PayPalApi.ClientId, targetEvent.PayPalApi.Secret);

                if (sendResult.IsSuccessful)
                {
                    invoiceRefNumber = draftResponse.id;
                }
                else
                {
                    _logger.LogError($"SendInvoiceAsync: {sendResult.ErrorMessage}");
                }
            }
            else
            {
                _logger.LogError($"SendInvoiceAsync: {draftResult.ErrorMessage}");
            }

            return invoiceRefNumber;
        }

        private PayPalInvoiceDraftRequest GenerateInvoiceRequest(EventModel targetEvent, RegistrationModel recipient, double amount)
        {
            return new PayPalInvoiceDraftRequest()
            {
                merchant_info = new PayPalMerchantInfo()
                {
                    email = targetEvent.PayPalApi.MerchantInfo.Email,
                    business_name = targetEvent.PayPalApi.MerchantInfo.BusinessName
                },
                billing_info = new List<PayPalBillingInfo>()
                    {
                        new PayPalBillingInfo()
                        {
                            email = recipient.Email,
                            first_name = recipient.FirstName,
                            last_name = recipient.LastName
                        }
                    },
                items = new List<PayPalItem>()
                    {
                        new PayPalItem()
                        {
                            name = targetEvent.PayPalApi.MerchantInfo.DonationName,
                            quantity = 1,
                            unit_price = new PayPalAmount()
                            {
                                currency = targetEvent.PayPalApi.MerchantInfo.Currency,
                                value = amount.ToString()
                            }
                        }
                    },
                note = targetEvent.PayPalApi.MerchantInfo.ThanksNote
            };
        }

        private Method PayPalLink2Method(PayPalResponseLink link)
        {
            if (link.method.Equals("GET"))
                return Method.GET;
            if (link.method.Equals("POST"))
                return Method.POST;
            if (link.method.Equals("PUT"))
                return Method.PUT;
            if (link.method.Equals("DELETE"))
                return Method.DELETE;
            throw new Exception($"PayPalLink2Method: Unsupported method {link.method}");
        }

        private async Task<IRestResponse<string>> CallApiAsync(Func<Task<IRestResponse<string>>> action, string apiClient, string apiSecret)
        {
            var retryPolicy = Policy
                .HandleResult<IRestResponse<string>>(r => r.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(1, async (exception, retryCount) =>
                {
                    try
                    {
                        await RefreshAccessTokenAsync(apiClient, apiSecret);
                    }
                    catch (Exception rte)
                    {
                        _logger.LogError($"CallApiAsync: {rte.Message}");
                    }
                });

            var result = await retryPolicy.ExecuteAsync(action);
            return result;
        }

        private async Task RefreshAccessTokenAsync(string apiClient, string apiSecret)
        {
            RestClient authClient = new RestClient(_config.Url);
            RestRequest request = new RestRequest("oauth2/token/", Method.POST);

            authClient.Authenticator = new HttpBasicAuthenticator(apiClient, apiSecret);
            authClient.Execute(request);

            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=client_credentials", ParameterType.RequestBody);

            var loginResult = await authClient.ExecuteTaskAsync<string>(request);
            if (loginResult.IsSuccessful)
            {
                var json = loginResult.Data;
                var jobj = JsonConvert.DeserializeObject<JObject>(json);
                var ppObj = jobj.ToObject<PayPalAccessResult>();

                _access_token = ppObj.access_token;
            }
            else
            {
                _logger.LogError($"RefreshAccessTokenAsync: {loginResult.ErrorMessage}");
            }
        }

        public void Dispose()
        {

        }


        /// <summary>
        /// PayPal Models
        /// </summary>

        public class PayPalInvoiceDraftRequest
        {
            public PayPalMerchantInfo merchant_info { get; set; }
            public List<PayPalBillingInfo> billing_info { get; set; }
            public List<PayPalItem> items { get; set; }
            public PayPalDiscount discount { get; set; }
            public PayPalShippingCost shipping_cost { get; set; }
            public string note { get; set; }
            public string terms { get; set; }
        }

        public class PayPalInvoiceDraftResponse : PayPalInvoiceDraftRequest
        {
            public string id { get; set; }
            public string number { get; set; }
            public string template_id { get; set; }
            public string status { get; set; }
            public string invoice_date { get; set; }
            public bool tax_calculated_after_discount { get; set; }
            public bool tax_inclusive { get; set; }
            public PayPalAmount total_amount { get; set; }
            public PayPalResponseMetadata metadata { get; set; }
            public bool allow_tip { get; set; }
            public List<PayPalResponseLink> links { get; set; }
        }

        public class PayPalMerchantInfo
        {
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string business_name { get; set; }
            public PayPalPhone phone { get; set; }
        }

        public class PayPalPhone
        {
            public string country_code { get; set; }
            public string national_number { get; set; }
        }

        public class PayPalBillingInfo
        {
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
        }

        public class PayPalItem
        {
            public string name { get; set; }
            public int quantity { get; set; }
            public PayPalAmount unit_price { get; set; }
            public PayPalTax tax { get; set; }
        }

        public class PayPalDiscount
        {
            public float percent { get; set; }
            public PayPalAmount amount { get; set; } // calculated by response methods
        }

        public class PayPalTax
        {
            public string name { get; set; }
            public float percent { get; set; }
            public PayPalAmount amount { get; set; } // calculated by response methods
        }

        public class PayPalShippingCost
        {
            public PayPalAmount amount { get; set; }
        }

        public class PayPalAmount
        {
            public string currency { get; set; }
            public string value { get; set; }
        }

        public class PayPalResponseMetadata
        {
            public string created_date { get; set; }
        }

        public class PayPalResponseLink
        {
            public string rel { get; set; }
            public string href { get; set; }
            public string method { get; set; }
        }

        public class PayPalAccessResult
        {
            public string scope { get; set; }
            public string nonce { get; set; }
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string app_id { get; set; }
            public string expires_in { get; set; }
        }
    }
}
