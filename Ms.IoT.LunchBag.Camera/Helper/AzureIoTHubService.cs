using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Ms.IoT.LunchBag.Camera.Models;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Diagnostics;

namespace Ms.IoT.LunchBag.Camera.Helper
{
    internal class AzureIoTHubService : IDisposable
    {
        private readonly LoggingChannel _logging;
        private DeviceClient _deviceClient;
        private DesiredPropertyUpdateCallback _callbackDesired;
        private Func<string, Task<bool>> _callbackAPI;

        internal bool Connected { get; private set; }

        internal AzureIoTHubService(LoggingChannel logging, DesiredPropertyUpdateCallback callbackDesired, Func<string, Task<bool>> callbackAPI)
        {
            _logging = logging;
            _callbackDesired = callbackDesired;
            _callbackAPI = callbackAPI;
        }

        internal async Task<bool> Init(string deviceConnectionString)
        {
            if (Connected)
                return true;

            _logging.LogMessage("Initializing IoT Hub Connection", LoggingLevel.Verbose);
            if (string.IsNullOrEmpty(deviceConnectionString))
            {
                _logging.LogMessage("Not DeviceConnectionString found.", LoggingLevel.Error);
                return false;
            }
            try
            {
                _deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

                Console.WriteLine("Registering Direct Method callbacks");
                await _deviceClient.SetMethodHandlerAsync("SetSubscriptionKey", OnSetSubscriptionKey, null);

                _logging.LogMessage("Registering Device Twin update callback", LoggingLevel.Verbose);
                await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(_callbackDesired, null);

                Connected = true;
            }
            catch(Exception e)
            {
                _logging.LogMessage($"AzureIoTHubService - Init: {e.Message}", LoggingLevel.Critical);
                Connected = false;
            }

            return true;
        }

        internal async Task SendDeviceToCloudMessageAsync(NoteMessage note)
        {
            if (_deviceClient == null)
                return;

            try
            {
                _logging.LogMessage($"Sending message to the cloud: {note.Note} for event {note.EventId}.", LoggingLevel.Verbose);
                var message = new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(note)));
                await _deviceClient.SendEventAsync(message);
            }
            catch (UnauthorizedException eu)
            {
                _logging.LogMessage($"SendDeviceToCloudMessageAsync: Unauthorized access. It can happen if the connection was lost.", LoggingLevel.Error);
                Connected = false;
            }
            catch(Exception e)
            {
                _logging.LogMessage($"SendDeviceToCloudMessageAsync: {e.Message}", LoggingLevel.Critical);
                Connected = false;
            }
        }

        internal async Task<string> GetDeviceTwinAsync()
        {
            _logging.LogMessage("Getting device twin", LoggingLevel.Verbose);
            try
            {
                Twin twin = await _deviceClient.GetTwinAsync();
                _logging.LogMessage(twin.ToJson(), LoggingLevel.Verbose);

                return twin.Properties.Desired.ToJson();
            }
            catch (UnauthorizedException eu)
            {
                _logging.LogMessage($"GetDeviceTwinAsync: Unauthorized access. It can happen if the connection was lost.", LoggingLevel.Error);
                Connected = false;
                return null;
            }
            catch (Exception e)
            {
                _logging.LogMessage($"GetDeviceTwinAsync: {e.Message}", LoggingLevel.Critical);
                Connected = false;
                return null;
            }
            
        }

        private async Task<MethodResponse> OnSetSubscriptionKey(MethodRequest methodRequest, object userContext)
        {
            _logging.LogMessage("OnSetSubscriptionKey has been called", LoggingLevel.Verbose);

            var key = JsonConvert.DeserializeObject<APISubscriptionKey>(methodRequest.DataAsJson);
            if (!string.IsNullOrEmpty(key.Key))
                _logging.LogMessage("Key empty", LoggingLevel.Verbose);
            await _callbackAPI(key.Key);
            
            return new MethodResponse(200);
        }

        public void Dispose()
        {
            if (_deviceClient != null)
            {
                _deviceClient.Dispose();
                _deviceClient = null;
                Connected = false;
            }
        }
    }
}