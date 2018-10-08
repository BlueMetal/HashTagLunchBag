using LunchBag.Common.IoTMessages;
using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using LunchBag.Common.Models.Transport.Bing;
using LunchBag.WebPortal.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.Api.Helpers
{
    public class RouteDataManager
    {
        private readonly WebPortalApiConfig _config;
        private readonly Dictionary<string, RouteModel> _dictRoutes;
        private readonly ICosmosDBRepository<RouteModel> _repoRoutes;
        private readonly ILogger<RouteDataManager> _logger;

        private readonly BingApiConfig _bingConfig;
        private readonly RestClient _bingClient;

        public RouteDataManager(IOptions<WebPortalApiConfig> config,
                                IOptions<BingApiConfig> bingConfig,
                                ICosmosDBRepository<RouteModel> repoRoutes,
                                ILogger<RouteDataManager> logger)
        {
            _logger = logger;
            _config = config.Value;
            _repoRoutes = repoRoutes;
            _dictRoutes = new Dictionary<string, RouteModel>();

            _bingConfig = bingConfig.Value;
            _bingClient = new RestClient(_bingConfig.Url);
        }

        public async Task<RouteModel> GetRoute(string routeId, bool forceRefresh = false)
        {
            RouteModel route = null;
            if (!_dictRoutes.ContainsKey(routeId) || !_config.UseCache || (_config.UseCache && forceRefresh))
            {
                route = await _repoRoutes.GetItemAsync(p => p.RouteId == routeId);
                if (route == null)
                    _logger.LogDebug($"GetRoute: No routes found for {routeId}.");
                _dictRoutes[routeId] = route;
            }
            else
            {
                route = _dictRoutes[routeId];
            }
            return route;
        }

        public async Task UpdateRoute(string vehicleId, string routeId, string latitude, string longitude)
        {
            RouteModel prev = await GetRoute(routeId);
            if (prev == null)
                prev = new RouteModel()
                {
                    RouteId = routeId,
                    Count = 0,
                    Points = new List<LocationModel>(),
                    RawCount = 0,
                    RawPoints = new List<LocationModel>()
                };

            List<LocationModel> rawPoints = prev.RawPoints;
            rawPoints.Add(new LocationModel()
            {
                latitude = decimal.Parse(latitude),
                longitude = decimal.Parse(longitude)
            });

            List<LocationModel> interpolatedPoints = new List<LocationModel>() { rawPoints[0] };
            if (rawPoints.Count > 1)
            {
                // take the raw points and interpolate them into real road coordinates
                //     - note that we use the full history of raw points as earlier interpolations may change based 
                //       on more recent data
                //     - note that we store the raw point history in the route model instead of querying mobiliya
                //       for the raw history to avoid both data lags and a dependency on mobiliya for route tracking
                interpolatedPoints = await InterpolateRoute(rawPoints);
            }

            await this.SaveUpdatedRoute(new RouteModel()
            {
                RouteId = routeId,
                Count = interpolatedPoints.Count,
                Points = interpolatedPoints,
                RawCount = rawPoints.Count,
                RawPoints = rawPoints,
            });

            if (_config.UseCache)
                await GetRoute(routeId, true);
        }

        public async Task<GeocodeModel> CalculateRouteDestination(string address)
        {
            RestRequest request = new RestRequest($"Locations", Method.GET);
            request.AddParameter("q", address, ParameterType.QueryString);
            request.AddParameter("o", "json", ParameterType.QueryString);
            request.AddParameter("key", _bingConfig.Key, ParameterType.QueryString);

            var result = await _bingClient.ExecuteTaskAsync<string>(request);
            if (!result.IsSuccessful)
                throw new Exception($"Failed to find address with error {result.StatusCode}");

            string json = result.Data;
            JObject jobj = JsonConvert.DeserializeObject<JObject>(json);
            BingGeocodeResult obj = jobj.ToObject<BingGeocodeResult>();
            BingGeocodeResource geo = obj.resourceSets[0].resources[0];

            return new GeocodeModel()
            {
                location = new LocationModel()
                {
                    latitude = geo.point.coordinates[0],
                    longitude = geo.point.coordinates[1]
                },
                confidence = geo.confidence,
                formattedAddress = geo.address.formattedAddress
            };
        }

        private async Task SaveUpdatedRoute(RouteModel route)
        {
            RouteModel prev = await _repoRoutes.GetItemAsync(p => p.RouteId == route.RouteId);
            if (prev == null)
            {
                await _repoRoutes.CreateItemAsync(route);
            }
            else
            {
                route.Id = prev.Id;
                await _repoRoutes.UpdateItemAsync(prev.Id, route);
            }
        }

        private async Task<List<LocationModel>> InterpolateRoute(List<LocationModel> rawPoints)
        {
            RestRequest request = new RestRequest($"Routes/SnapToRoad?key={_bingConfig.Key}", Method.POST);
            request.AddParameter("application/json",
                                 JsonConvert.SerializeObject(new BingSnapBody()
                                 {
                                     points = rawPoints
                                 }),
                                 ParameterType.RequestBody);

            var result = await _bingClient.ExecuteTaskAsync<string>(request);
            if (!result.IsSuccessful)
                throw new Exception($"Failed to interpolate route via bing snap with error {result.ErrorMessage}");

            var responseJson = result.Data;
            var jObject = JObject.Parse(responseJson);

            var bingResult = jObject.ToObject<BingSnapResult>();
            if (bingResult == null || !string.Equals(bingResult.statusDescription, "OK"))
                throw new Exception("Failed to snap points");

            var snappedPoints = bingResult.resourceSets[0].resources[0].snappedPoints.Select(x => new LocationModel()
            {
                latitude = x.coordinate.latitude,
                longitude = x.coordinate.longitude
            }).ToList();

            if (snappedPoints.Count == 0)
                throw new Exception("No points returned from bing snap");

            return snappedPoints;
        }
    }
}
