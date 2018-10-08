using LunchBag.Common.Config;
using LunchBag.Common.IoTMessages;
using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using LunchBag.SimulatorApp.Config;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using LunchBag.Common.Models.Transport.Bing;
using Newtonsoft.Json.Linq;
using RestSharp.Authenticators;
using System.Net;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading;

namespace LunchBag.MobiliyaSimulator
{
    public class Application : IDisposable
    {
        private readonly ILogger<Application> _logger;
        private readonly SimulationConfig _config;
        private readonly RestServiceOptions _restConfig;

        private RestClient _restClient;
        private RestClient _bingClient;

        private readonly string _clientId;
        private readonly AuthenticationContext _authContext = null;
        private readonly ClientCredential _clientCredential = null;

        private Random _rand = new Random();
        private System.Timers.Timer _timer;
        private DeviceClient _deviceClient = null;

        private List<RunningDelivery> _runningDeliveries;
        private RunningDelivery _runningDelivery;

        private ConsoleColor[] _consoleColors = {ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Cyan,
                                                 ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Yellow};

        private enum SimOptions { eListDeliveries, eRunDelivery, eResetDelivery, eQuit };
        private Dictionary<int, KeyValuePair<string, string>> _optionsList
            = new Dictionary<int, KeyValuePair<string, string>>()
        {
            { (int)SimOptions.eListDeliveries, new KeyValuePair<string, string>( "list", "List all available deliveries in the system" ) },
            { (int)SimOptions.eRunDelivery, new KeyValuePair<string, string>( "deliver", "Execute a simulation of a delivery or deliveries" ) },
            { (int)SimOptions.eResetDelivery, new KeyValuePair<string, string>( "reset", "Reset the state of a delivery or deliveries" ) },
            { (int)SimOptions.eQuit, new KeyValuePair<string, string>( "quit", "Quit the simulator" ) }
        };

        public Application(ILogger<Application> logger, 
                           IOptions<SimulationConfig> config,
                           IOptions<RestServiceOptions> restConfig,
                           IOptions<AzureAdOptions> azureConfig)
        {
            _logger = logger;
            _config = config.Value;
            _restConfig = restConfig.Value;

            _restClient = new RestClient(_restConfig.RestServiceUrl);
            _restClient.Authenticator = new HttpBasicAuthenticator("admin", "lunchbag1234");
            _bingClient = new RestClient(_config.BingUrl);

            // all deliveries/vehicles will use a single connection string during simulation
            string connstr = _config.ConnectionString;
            _deviceClient = DeviceClient.CreateFromConnectionString(connstr, TransportType.Mqtt);

            _clientId = azureConfig.Value.ClientId;
            _authContext = new AuthenticationContext(azureConfig.Value.Instance + azureConfig.Value.TenantId);
            _clientCredential = new ClientCredential(azureConfig.Value.ClientId, azureConfig.Value.ClientSecret);
        }

        public async Task Run()
        {
            try
            {
                Console.WriteLine("Usage -->>");
                ListOptions();

                while (true)
                {
                    string choice = Console.ReadLine();
                    int chosen = -1;
                    
                    foreach (var option in _optionsList)
                    {
                        if (choice.Equals(option.Value.Key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            chosen = option.Key;
                            break;
                        }
                    }

                    if (chosen == (int)SimOptions.eQuit)
                        break;

                    if (chosen == (int)SimOptions.eListDeliveries)
                        await this.ListDeliveries();
                    else if (chosen == (int)SimOptions.eRunDelivery)
                        await this.RunDelivery();
                    else if (chosen == (int)SimOptions.eResetDelivery)
                        await this.ResetDelivery();
                    else
                        Console.WriteLine("Invalid Choice!  Try again.");

                    Console.WriteLine(string.Empty);
                    Console.WriteLine("Usage -->>");
                    ListOptions();

                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    _timer = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private async Task RunDelivery()
        {
            ClearTimer();

            try
            {
                Console.WriteLine("Enter the Event Id");
                string eventId = Console.ReadLine();

                Console.WriteLine("Enter the Delivery Id. Use comma-separated list for multiple deliveries.");
                string deliveryCSV = Console.ReadLine();
                string[] deliveries = deliveryCSV.Split(',');
                
                if (deliveries.Count() == 0)
                    throw new Exception("Could not understand input string");

                _runningDeliveries = new List<RunningDelivery>();
                foreach (string d in deliveries)
                {
                    string deliveryId = d;

                    DeliveryModel delivery = await GetDelivery(eventId, deliveryId);
                    if (delivery == null)
                        throw new Exception("Invalid event or delivery id.  Please try again.");

                    BingGeocodeResource destination = await Geocode(delivery.Destination);
                    if (destination == null)
                        throw new Exception("Could not geocode destination.  Please try again.");

                    string city = destination.address.locality;
                    string state = destination.address.adminDistrict;

                    BingGeocodeResource cityCenter = await Geocode($"{city}, {state}");
                    if (cityCenter == null)
                        throw new Exception("Could not find a suitable delivery start location");

                    var bingout = await ComputeRoute(cityCenter.point, destination.point);
                    var distance = bingout.routeLegs[0].travelDistance;
                    var startLat = bingout.routePath.line.coordinates[0][0];
                    var startLong = bingout.routePath.line.coordinates[0][1];

                    var coords = ReduceCoordinateResolution(bingout.routePath.line.coordinates, 
                                                            bingout.routePath.generalizations[0].pathIndices);
                    var numPoints = coords.Count;

                    RunningDelivery runningDelivery = new RunningDelivery()
                    {
                        _deviceClient = _deviceClient,
                        _delivery = delivery,
                        _tripId = Guid.NewGuid().ToString(),
                        _vehicleId = delivery.VehicleId,
                        _driverId = delivery.DriverId,
                        _distanceIncrement = distance / (decimal)numPoints,
                        _locations = coords.ConvertAll(x => new MobiliyaLocation
                        {
                            Latitude = x[0].ToString(),
                            Longitude = x[1].ToString()
                        })
                    };
                    _runningDeliveries.Add(runningDelivery);
                }

                Console.WriteLine("** Ready To Send Simulated Route Points **");
                ListSimulationOptions();

                while (true)
                {
                    char keychar = Console.ReadKey().KeyChar;
                    if (keychar == ' ')
                    {
                        if (_timer != null)
                        {
                            ClearTimer();
                            Console.WriteLine("Auto Play Terminated.");
                        }
                        await SendDeliveryUpdates(null, null, null);
                    }
                    else if (keychar == 'a')
                    {
                        System.Timers.Timer newTimer = new System.Timers.Timer();
                        newTimer.Elapsed += async(sender, e) => await SendDeliveryUpdates(sender, e, newTimer);
                        newTimer.Interval = _config.FleetUpdateSpan;
                        newTimer.Start();
                        _timer = newTimer;

                        ListSimulationOptions();
                    }
                    else if (keychar == 'q')
                    {
                        Console.WriteLine("Simulation Terminated");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Not a valid option.");
                        ListSimulationOptions();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task SendDeliveryUpdates(object source, ElapsedEventArgs e, System.Timers.Timer currentTimer)
        {
            int iColor = 0;
            try
            {
                if (_timer != null) { _timer.Stop(); } // stop the timer while sending multiple position updates to avoid overlaps

                Console.WriteLine(string.Empty);
                Console.WriteLine($"SENDING UPDATES FOR {_runningDeliveries.Count} DELIVERIES.");
                foreach (RunningDelivery rd in _runningDeliveries)
                {
                    _runningDelivery = rd;

                    Console.ForegroundColor = _consoleColors[iColor % _consoleColors.Length];
                    Console.WriteLine($"Sending update for {rd._delivery.DeliveryId}");

                    await rd.SendDeliveryUpdate();
                    await Task.Delay(_config.CarUpdateSpan);

                    iColor++;
                }
                Console.ResetColor();

                Console.WriteLine(string.Empty);
                Console.WriteLine("All Updates Complete");
                var activeDeliveries = _runningDeliveries.Where(p => p._iLocation < p._locations.Count);

                if (_timer != null) { _timer.Start(); } // restart to wait _config.TimerSpan before sending the next batch

                if (activeDeliveries.Count() > 0)
                    ListSimulationOptions();
                else
                    Console.WriteLine("All Deliveries Exhaused.  Press [q] to quit simulation.");
            }
            catch (Exception ex)
            {
                Console.ResetColor();
                throw ex;
            }
        }

        private class RunningDelivery
        {
            public DeliveryModel _delivery { get; set; }
            public string _tripId { get; set; }
            public string _vehicleId { get; set; }
            public string _driverId { get; set; }
            public decimal _distanceIncrement { get; set; }
            public List<MobiliyaLocation> _locations { get; set; }
            public int _iLocation { get; set; }
            public DeviceClient _deviceClient { get; set; }

            public async Task SendDeliveryUpdate()
            {
                if (_iLocation >= _locations.Count)
                {
                    Console.WriteLine("Trip data exhausted.  Delivery over.");
                    Console.WriteLine("Press [q] to quit the simulation.");
                    return;
                }

                MobiliyaLocation currentLocation = _locations[_iLocation];
                currentLocation.Distance = _iLocation * _distanceIncrement;

                DeliveryIoTMessage iotMessage = new DeliveryIoTMessage()
                {
                    Distance = currentLocation.Distance,
                    Latitude = currentLocation.Latitude,
                    Longitude = currentLocation.Longitude,

                    TripId = _tripId,
                    UserId = _driverId,
                    VehicleId = _vehicleId,
                    VehicleRegNumber = _delivery.DeliveryId, 
                    ParameterDateTime = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                    ID = _iLocation + 1
                };

                string raw = JsonConvert.SerializeObject(iotMessage);
                raw = "{\"Parameters\":" + "\"" + raw.Replace("\"", "\\\"") + "\"" + "}";
                var message = new Message(Encoding.ASCII.GetBytes(raw));

                Console.WriteLine($"Updating trip {_tripId}");
                {

                    await _deviceClient.SendEventAsync(message);
                    Console.WriteLine("Message Sent " + raw);
                }

                ++_iLocation;
            }
        }

        private async Task ListDeliveries()
        {
            try
            {
                var events = await GetEvents();
                var eventList = events?.ToList<EventModel>();

                if (eventList == null || eventList.Count == 0)
                    throw new Exception("No events found - make sure to create an event in the portal");

                foreach (EventModel ev in eventList)
                {
                    var deliveries = await GetDeliveries(ev.Id);

                    if (deliveries == null || deliveries.Count == 0)
                        throw new Exception("No deliveries found - make sure to create a delivery in the portal");

                    foreach (DeliveryModel delivery in deliveries.Deliveries)
                    {
                        string address = delivery.DestinationGeocode?.formattedAddress;
                        if (address == null)
                            address = delivery.Destination;

                        Console.WriteLine(delivery.DeliveryId);
                        Console.WriteLine($"\tEvent:\t{delivery.EventId}");
                        Console.WriteLine($"\tAddress:\t{address}");
                        Console.WriteLine($"\tDriver:\t{delivery.DriverName}");
                        Console.WriteLine($"\tStatus:\t{delivery.Status}");
                        Console.WriteLine($"\tRouteId:\t{delivery.RouteId}");
                        Console.WriteLine("==================================");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ResetDelivery()
        {
            ClearTimer();

            try
            {
                Console.WriteLine("Enter the Event Id");
                string eventId = Console.ReadLine();

                Console.WriteLine("Enter the Delivery Id. Use comma-separated list for multiple deliveries.");
                string deliveryCSV = Console.ReadLine();
                string[] deliveries = deliveryCSV.Split(',');

                if (deliveries.Count() == 0)
                    throw new Exception("Could not understand input string");

                foreach (string d in deliveries)
                {
                    string deliveryId = d;

                    DeliveryModel delivery = await GetDelivery(eventId, deliveryId);
                    if (delivery == null)
                        throw new Exception("Invalid event or delivery id.  Please try again.");

                    delivery.RouteId = null;
                    delivery.MilesToDestination = 0;
                    delivery.MilesTraveled = 0;
                    delivery.Status = DeliveryModel.kDeliveryStatusNotStarted;

                    Console.WriteLine($"Resetting {delivery.DeliveryId}");
                    await this.UpateDelivery(delivery);
                }

                Console.WriteLine("** Done **");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<BingGeocodeResource> Geocode(string address)
        {
            RestRequest request = new RestRequest($"Locations", Method.GET);
            request.AddParameter("q", address, ParameterType.QueryString);
            request.AddParameter("o", "json", ParameterType.QueryString);
            request.AddParameter("key", _config.BingKey, ParameterType.QueryString);

            var result = await _bingClient.ExecuteTaskAsync<string>(request);
            if (!result.IsSuccessful)
                throw new Exception($"Failed to find address with error {result.StatusCode}");

            string json = result.Data;
            JObject jobj = JsonConvert.DeserializeObject<JObject>(json);
            BingGeocodeResult obj = jobj.ToObject<BingGeocodeResult>();
            BingGeocodeResource geo = obj.resourceSets[0].resources[0];

            return geo;
        }

        private async Task<BingRouteResource> ComputeRoute(BingGeocodePoint start, BingGeocodePoint end)
        {
            RestRequest request = new RestRequest($"Routes/Driving", Method.GET);
            request.AddParameter("wp.0", $"{start.coordinates[0]},{start.coordinates[1]}", ParameterType.QueryString);
            request.AddParameter("wp.1", $"{end.coordinates[0]},{end.coordinates[1]}", ParameterType.QueryString);
            request.AddParameter("routeAttributes", "routePath", ParameterType.QueryString);
            request.AddParameter("distanceUnit", "mi", ParameterType.QueryString);
            request.AddParameter("tl", _config.BingTolerance.ToString(), ParameterType.QueryString);
            request.AddParameter("key", _config.BingKey, ParameterType.QueryString);

            var result = await _bingClient.ExecuteTaskAsync<string>(request);
            if (!result.IsSuccessful)
                throw new Exception($"Failed to find address with error {result.StatusCode}");

            string json = result.Data;
            JObject jobj = JsonConvert.DeserializeObject<JObject>(json);
            BingRouteResult obj = jobj.ToObject<BingRouteResult>();
            BingRouteResource res = obj.resourceSets[0].resources[0];

            return res;
        }

        private List<List<decimal>> ReduceCoordinateResolution(List<List<decimal>> highRes, List<int> reducedIndices)
        {
            List<List<decimal>> reduced = new List<List<decimal>>();

            foreach (int targetIndex in reducedIndices)
                reduced.Add(highRes[targetIndex]);

            while (reduced.Count > 100)
                reduced = (reduced.Where((value, index) => (index % 2) == 0)).ToList();

            return reduced;
        }

        private async Task<IEnumerable<EventModel>> GetEvents()
        {
            RestRequest request = await GetSecureRestRequest("Events", Method.GET);
            
            var queryResult = await _restClient.ExecuteTaskAsync<IEnumerable<EventModel>>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                Console.WriteLine($"GetEvents: {queryResult.ErrorMessage}");
            return null;
        }

        private async Task<DeliveriesModel> GetDeliveries(string eventId)
        {
            RestRequest request = await GetSecureRestRequest("Transport/{eventId}/deliveries", Method.GET);
            request.AddUrlSegment("eventId", eventId);

            var queryResult = await _restClient.ExecuteTaskAsync<DeliveriesModel>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                Console.WriteLine($"GetDeliveriesPerEvent: {queryResult.ErrorMessage}");
            return null;
        }

        private async Task<DeliveryModel> GetDelivery(string eventId, string deliveryId)
        {
            RestRequest request = await GetSecureRestRequest("Transport/{eventId}/deliveries/{deliveryId}", Method.GET);
            request.AddUrlSegment("eventId", eventId);
            request.AddUrlSegment("deliveryId", deliveryId);

            var queryResult = await _restClient.ExecuteTaskAsync<DeliveryModel>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                Console.WriteLine($"GetDeliveryPerEventId: {queryResult.ErrorMessage}");
            return null;
        }

        private async Task<HttpStatusCode> UpateDelivery(DeliveryModel delivery)
        {
            RestRequest request = await GetSecureRestRequest("Transport/{eventId}/deliveries/{deliveryId}", Method.PUT);
            request.AddUrlSegment("eventId", delivery.EventId);
            request.AddUrlSegment("deliveryId", delivery.DeliveryId);
            request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(delivery),
                                 ParameterType.RequestBody);

            var putResult = await _restClient.ExecuteTaskAsync(request);
            if (!putResult.IsSuccessful)
                Console.WriteLine($"UpateDelivery: Error while sending a message: {putResult.StatusCode}");
            return putResult.StatusCode;
        }

        private async Task<string> GetAzureAdToken()
        {
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            LoggerCallbackHandler.UseDefaultLogging = false;
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
            LoggerCallbackHandler.UseDefaultLogging = true;

            return result?.AccessToken;
        }

        private async Task<RestRequest> GetSecureRestRequest(string endpoint, Method method)
        {
            var token = await GetAzureAdToken();

            RestRequest request = new RestRequest(endpoint, method);
            request.AddHeader("Authorization", $"Bearer {token}");

            return request;
        }

        private void ListOptions()
        {
            foreach (var option in _optionsList)
            {
                Console.WriteLine($"\t{option.Value.Key}\t- {option.Value.Value}");
            }
            Console.WriteLine(string.Empty);
        }

        private void ListSimulationOptions()
        {
            Console.WriteLine(string.Empty);
            if (_timer != null)
            {
                Console.WriteLine($"Sending fleet updates every {_config.FleetUpdateSpan / 1000} seconds...");
                Console.WriteLine($"Cars in fleet separated by {_config.CarUpdateSpan / 1000} seconds...");
                Console.WriteLine("Press [space] to switch to manual mode.");
                Console.WriteLine("- OR - Press [q] at ant point to quit the simulation");
            }
            else
            {
                Console.WriteLine("Press [space] to send the next point.");
                Console.WriteLine("- OR - Press [a] to automate the simulation.");
                Console.WriteLine("- OR - Press [q] at ant point to quit the simulation");
            }
        }

        private double CalculateDistance(double lat1, double long1, double lat2, double long2)
        {
            const double degreesToRadians = (Math.PI / 180.0);
            const double earthRadius = 6371; // kilometers

            // convert latitude and longitude values to radians
            var radLat1 = lat1 * degreesToRadians;
            var radLong1 = long1 * degreesToRadians;
            var radLat2 = lat2 * degreesToRadians;
            var radLong2 = long2 * degreesToRadians;

            // calculate radian delta between each position.
            var radDeltaLat = radLat2 - radLat1;
            var radDeltaLong = radLong2 - radLong1;

            // calculate distance
            var expr1 = (Math.Sin(radDeltaLat / 2.0) *
                         Math.Sin(radDeltaLat / 2.0)) +
                        (Math.Cos(radLat1) *
                         Math.Cos(radLat2) *
                         Math.Sin(radDeltaLong / 2.0) *
                         Math.Sin(radDeltaLong / 2.0));

            var expr2 = 2.0 * Math.Atan2(Math.Sqrt(expr1),
                                         Math.Sqrt(1 - expr1));

            var distanceInKilometers = (earthRadius * expr2);
            var distanceInMiles = distanceInKilometers * 0.62137;

            return distanceInMiles;
        }

        public void ClearTimer()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
                _timer = null;
            }
            catch (Exception)
            {

            }
        }

        public void Dispose()
        {
            ClearTimer();
        }
    }

    public class MobiliyaTrip
    {
        public List<MobiliyaLocation> Locations { get; set; }
    }

    public class MobiliyaLocation
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal Distance { get; set; }
    }
}
