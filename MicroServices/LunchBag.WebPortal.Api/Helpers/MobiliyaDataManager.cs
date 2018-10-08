using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LunchBag.Common.Config;
using LunchBag.Common.IoTMessages;
using LunchBag.Common.Managers;
using LunchBag.Common.Models.Transport;
using LunchBag.WebPortal.Api.Config;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RestSharp;

namespace LunchBag.WebPortal.Api.Helpers
{
    public class MobiliyaDataManager : IDisposable
    {
        private readonly MobiliyaApiConfig _config;
        private readonly ILogger<MobiliyaDataManager> _logger;
        private readonly ICacheService _cacheService;
        private readonly RestClient _tripClient;
        private readonly RestClient _fleetClient;
        private readonly RestClient _identityClient;

        private static readonly string TOKEN_REDIS_KEY = "MobiliyaToken";
        private static readonly string TOKEN_REDIS_DEFAULT = "default_token";
        private static readonly string TENANT_REDIS_KEY = "MobiliyaTenant";
        private static readonly string TENANT_REDIS_DEFAULT = "default_tenant";

        public MobiliyaDataManager(IOptions<MobiliyaApiConfig> config,
                                   ILogger<MobiliyaDataManager> logger,
                                   ICacheService cacheService)
        {
            _logger = logger;
            _config = config.Value;
            _cacheService = cacheService;
            _tripClient = new RestClient(_config.TripService);
            _fleetClient = new RestClient(_config.FleetService);
            _identityClient = new RestClient(_config.IdentityService);
        }

        public async Task<FleetModel> GetEventVehicles(string eventId)
        {
            if (_config.UseSimulation)
                return GetSimulatedEventVehicles(eventId);

            MobiliyaFleetSummary fleet = await FindFleetByEventId(eventId);
            List<MobiliyaFleetVehicle> vehicles = await FindVehiclesByFleetId(fleet.id);
            List<MobiliyaFleetDriver> drivers = await FindDriversByFleetId(fleet.id);

            FleetModel fleetmodel = new FleetModel()
            {
                FleetId = fleet.id,
                VehicleCount = vehicles.Count,
                Vehicles = new List<FleetVehicle>()
            };

            foreach (MobiliyaFleetVehicle vehicle in vehicles)
            {
                ((List<FleetVehicle>)fleetmodel.Vehicles).Add(new FleetVehicle()
                {
                    VehicleId = vehicle.id,
                    VehicleName = $"{vehicle.brandName} {vehicle.model}",
                    DriverId = vehicle.userId,
                    DriverName = GetDriverById(drivers, vehicle.userId)
                });
            }

            return fleetmodel;
        }

        private async Task<IRestResponse<string>> QueryAllFleetsAsync()
        {
            var result = await CallApiAsync(async () =>
            {
                string token = await GetCachedToken();
                string tenant = await GetCachedTenant();

                RestRequest request = new RestRequest($"{tenant}/fleets", Method.GET);
                request.AddParameter("Authorization", token, ParameterType.HttpHeader);

                var mobresult = await _fleetClient.ExecuteTaskAsync<string>(request);
                return mobresult;
            });
            return result;
        }

        private async Task<IRestResponse<string>> QueryFleetVehiclesAsync(string fleetId)
        {
            var result = await CallApiAsync(async () =>
            {
                string token = await GetCachedToken();
                string tenant = await GetCachedTenant();

                RestRequest request = new RestRequest($"{tenant}/vehicles?fleetId={fleetId}", Method.GET);
                request.AddParameter("Authorization", token, ParameterType.HttpHeader);

                var mobresult = await _fleetClient.ExecuteTaskAsync<string>(request);
                return mobresult;
            });
            return result;
        }

        private async Task<IRestResponse<string>> QueryFleetDriversAsync(string fleetId)
        {
            var result = await CallApiAsync(async () =>
            {
                string token = await GetCachedToken();
                string tenant = await GetCachedTenant();

                RestRequest request = new RestRequest($"users?fleetId={fleetId}", Method.GET);
                request.AddParameter("Authorization", token, ParameterType.HttpHeader);

                var mobresult = await _identityClient.ExecuteTaskAsync<string>(request);
                return mobresult;
            });
            return result;
        }

        private async Task<IRestResponse<string>> CallApiAsync(Func<Task<IRestResponse<string>>> action)
        {
            var retryPolicy = Policy
                .HandleResult<IRestResponse<string>>(r => r.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(1, async (exception, retryCount) =>
                {
                    try
                    {
                        await RefreshAccessTokenAsync();
                    }
                    catch (Exception rte)
                    {
                        _logger.LogError($"CallApiAsync: {rte.Message}");
                    }
                });

            var result = await retryPolicy.ExecuteAsync(action);
            return result;
        }

        private async Task<string> GetCachedToken()
        {
            string token = await _cacheService.Get<string>(TOKEN_REDIS_KEY);
            if (string.IsNullOrEmpty(token)) { token = TOKEN_REDIS_DEFAULT; }

            return token;
        }

        private async Task<string> GetCachedTenant()
        {
            string tenant = await _cacheService.Get<string>(TENANT_REDIS_KEY);
            if (string.IsNullOrEmpty(tenant)) { tenant = TENANT_REDIS_DEFAULT; }

            return tenant;
        }

        private async Task RefreshAccessTokenAsync()
        {
            RestRequest request = new RestRequest("login", Method.POST);

            dynamic postbod = new JObject();
            postbod.email = _config.Username;
            postbod.password = _config.Password;
            request.AddParameter("application/json", postbod.ToString(), ParameterType.RequestBody);

            var loginResult = await _identityClient.ExecuteTaskAsync<string>(request);
            if (loginResult.IsSuccessful)
            {
                var json = loginResult.Data;
                var jobj = JsonConvert.DeserializeObject<JObject>(json);

                var message = jobj.GetValue("message");
                if (!string.Equals(message.ToObject<string>(), "success", StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Malformed login result");

                var data = jobj.GetValue("data");
                var jdata = data as JObject;
                var token = jdata.GetValue("access_token");
                var stoken = token.ToObject<string>();

                var deets = jdata.GetValue("userDetails");

                var jdeets = deets as JObject;
                var tenant = jdeets.GetValue("tenantId");
                var stenant = tenant.ToObject<string>();

                await _cacheService.Set(TOKEN_REDIS_KEY, stoken);
                await _cacheService.Set(TENANT_REDIS_KEY, stenant);
            }
            else
            {
                _logger.LogError($"RefreshAccessTokenAsync: {loginResult.ErrorMessage}");
            }
        }

        private async Task<MobiliyaFleetSummary> FindFleetByEventId(string eventId)
        {
            IRestResponse<string> response = await QueryAllFleetsAsync();
            if (!response.IsSuccessful)
                throw new Exception(response.ErrorMessage);

            var responseJson = response.Data;
            var jObject = JObject.Parse(responseJson);
            var message = jObject.GetValue("message");

            if (!string.Equals(message.ToObject<string>(), "Success"))
                throw new HttpRequestException($"No fleets found. Check Mobiliya config.");

            var data = jObject.GetValue("data");
            var dataArray = data.ToObject<JArray>();

            List<MobiliyaFleetSummary> fleets = dataArray.ToObject<List<MobiliyaFleetSummary>>();
            MobiliyaFleetSummary fleet
                = fleets.FirstOrDefault(p => p.fleetName.Equals(eventId, StringComparison.InvariantCultureIgnoreCase));

            if (fleet == null && eventId.Equals("LasVegas", StringComparison.InvariantCultureIgnoreCase)) // demo case
                fleet = fleets.FirstOrDefault(p => p.fleetName.Equals("SampleFleet", StringComparison.InvariantCultureIgnoreCase));

            if (fleet == null)
                throw new HttpRequestException($"No fleet found for event {eventId}. Create a fleet for this event.");

            return fleet;
        }

        private async Task<List<MobiliyaFleetVehicle>> FindVehiclesByFleetId(string fleetId)
        {
            IRestResponse<string> response = await QueryFleetVehiclesAsync(fleetId);
            if (!response.IsSuccessful)
                throw new Exception(response.ErrorMessage);

            var responseJson = response.Data;
            var jObject = JObject.Parse(responseJson);
            var message = jObject.GetValue("message");

            if (!string.Equals(message.ToObject<string>(), "Success"))
                throw new HttpRequestException($"No fleets found. Check Mobiliya config.");

            var data = jObject.GetValue("data");
            var dataArray = data.ToObject<JArray>();

            List<MobiliyaFleetVehicle> vehicles = dataArray.ToObject<List<MobiliyaFleetVehicle>>();
            return vehicles;
        }

        private async Task<List<MobiliyaFleetDriver>> FindDriversByFleetId(string fleetId)
        {
            IRestResponse<string> response = await QueryFleetDriversAsync(fleetId);
            if (!response.IsSuccessful)
                throw new Exception(response.ErrorMessage);

            var responseJson = response.Data;
            var jObject = JObject.Parse(responseJson);
            var message = jObject.GetValue("message");

            if (!string.Equals(message.ToObject<string>(), "Success"))
                throw new HttpRequestException($"No fleets found. Check Mobiliya config.");

            var data = jObject.GetValue("data");
            var dataArray = data.ToObject<JArray>();

            List<MobiliyaFleetDriver> drivers = dataArray.ToObject<List<MobiliyaFleetDriver>>();
            return drivers;
        }

        private string GetDriverById(List<MobiliyaFleetDriver> drivers, string id)
        {
            MobiliyaFleetDriver driver = drivers.FirstOrDefault(p => p.id.Equals(id));
            if (driver == null)
                return null;

            return $"{driver.firstName} {driver.lastName}".Trim();
        }

        private FleetModel GetSimulatedEventVehicles(string eventId)
        {
            List<FleetVehicle> vehicles = new List<FleetVehicle>()
            {
                new FleetVehicle()
                {
                    DriverId = "SimDriverId_1",
                    DriverName = "Driver One",
                    VehicleId = "SimVehicleId_1",
                    VehicleName = "Vehicle One"
                },
                new FleetVehicle()
                {
                    DriverId = "SimDriverId_2",
                    DriverName = "Driver Two",
                    VehicleId = "SimVehicleId_2",
                    VehicleName = "Vehicle Two"
                },
                new FleetVehicle()
                {
                    DriverId = "SimDriverId_3",
                    DriverName = "Driver Three",
                    VehicleId = "SimVehicleId_3",
                    VehicleName = "Vehicle Three"
                },
                new FleetVehicle()
                {
                    DriverId = "SimDriverId_4",
                    DriverName = "Driver Four",
                    VehicleId = "SimVehicleId_4",
                    VehicleName = "Vehicle Four"
                }
            };

            return new FleetModel()
            {
                FleetId = eventId,
                VehicleCount = vehicles.Count,
                Vehicles = vehicles
            };
        }

        public void Dispose()
        {

        }

        private class MobiliyaFleetSummary
        {
            public string id { get; set; }
            public string fleetName { get; set; }
            public string description { get; set; }
            public string tenantId { get; set; }
            public string fleetAdminId { get; set; }
            public bool isDeleted { get; set; }
            public string status { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
        }

        private class MobiliyaFleetVehicle
        {
            public string id { get; set; }
            public string brandName { get; set; }
            public string model { get; set; }
            public string fuelType { get; set; }
            public string registrationNumber { get; set; }
            public string yearOfManufacture { get; set; }
            public string color { get; set; }
            public string userId { get; set; }
            public string deviceId { get; set; }
            public string fleetId { get; set; }
            public bool isDeleted { get; set; }
            public string status { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
        }

        private class MobiliyaFleetDriver
        {
            public string id { get; set; }
            public string email { get; set; }
            public string password { get; set; }
            public string mobileNumber { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public int status { get; set; }
            public string roleId { get; set; }
            public string fleetId { get; set; }
            public string tenantId { get; set; }
            public int isDriverAssign { get; set; }
            public string licenseNumber { get; set; }
        }
    }
}
