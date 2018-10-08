using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Ms.IoT.LunchBag.Camera.Helper;
using Windows.Storage;
using Ms.IoT.LunchBag.Camera.Config;
using Windows.Foundation.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Ms.IoT.LunchBag.Camera.Models;
using Microsoft.Devices.Tpm;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Ms.IoT.LunchBag.Camera
{
    public sealed class StartupTask : IBackgroundTask
    {
        private ApplicationDataContainer _localSettings;
        private LoggingChannel _logging;
        private TpmDevice _tpmDevice;
        private AzureIoTHubService _azureIoTHubService;
        private CameraService _cameraService;
        private CognitiveVisionService _cognitiveService;
        private LunchBagCameraSettings _config;
        private bool _isRunning = false;
        private bool _isInterruptRequested = false;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //Deferral task to allow for async
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            //Config
            _localSettings = ApplicationData.Current.LocalSettings;

            //Get TPM Device ConnectionString
            _tpmDevice = new TpmDevice(0);

            //Create services
            _logging = new LoggingChannel("LunchBagCamera", null, new Guid("8e2fb18a-e7ae-45f9-bf05-d42455ba6ce0"));
            _azureIoTHubService = new AzureIoTHubService(_logging, ReceiveConfiguration, ReceiveSubscription);
            _cognitiveService = new CognitiveVisionService(_logging);
            _cameraService = new CameraService(_logging);

            await RunApplication();

            deferral.Complete();
        }

        private async Task RunApplication()
        {
            _logging.LogMessage("Initializing Application", LoggingLevel.Verbose);

            //Set up IoT Hub Device
            if (!await _azureIoTHubService.Init(_tpmDevice.GetConnectionString()))
            {
                _logging.LogMessage("The application was not initialized. Please make sure that the TPM service is properly set up on Device 0. Then restart the application.", LoggingLevel.Error);
                return;
            }

            await InitConfig();
            await _cameraService.Init(_config.CameraIndex, _config.CameraResolution);
            _cognitiveService.Init(_config.CognitiveVisionAPIKey);

            //Run the program
            _isRunning = true;
            do
            {
                _isInterruptRequested = false;
                if (_cameraService.IsInitialized && _azureIoTHubService.Connected &&
                    !string.IsNullOrEmpty(_config.EventId) && !string.IsNullOrEmpty(_config.CognitiveVisionAPIKey))
                {
                    using (var captureStream = new InMemoryRandomAccessStream())
                    {
                        await _cameraService.TakePhotoToStream(captureStream);
                        if (captureStream.Position != 0)
                        {
                            string text = await _cognitiveService.ProcessPhotoFromStream(captureStream);
                            if (!string.IsNullOrEmpty(text))
                            {
                                await _azureIoTHubService.SendDeviceToCloudMessageAsync(new NoteMessage()
                                {
                                    EventId = _config.EventId,
                                    Note = text,
                                    Type = "Note"
                                });
                            }
                            else
                            {
                                _logging.LogMessage("No note was found.", LoggingLevel.Verbose);
                            }
                        }
                    }
                }
                else
                {
                    _isInterruptRequested = true;
                    _config.IntervalPhoto = 5;
                }
                await Task.Delay(_config.IntervalPhoto * 1000);
            }
            while (!_isInterruptRequested);

            //Program interrupted, dispose the services
            if(!_azureIoTHubService.Connected)
                _azureIoTHubService.Dispose();
            _cameraService.Dispose();

            _isRunning = false;

            //Restart
            await RunApplication();
        }

        private async Task InitConfig()
        {
            try
            {
                _logging.LogMessage("Initializing Configuration", LoggingLevel.Verbose);
                var desiredProperties = await _azureIoTHubService.GetDeviceTwinAsync();
                if (desiredProperties == null)
                    _config = new LunchBagCameraSettings();
                else
                    _config = JsonConvert.DeserializeObject<LunchBagCameraSettings>(desiredProperties);
            }
            catch (Exception e)
            {
                _logging.LogMessage($"InitConfig: {e.Message}", LoggingLevel.Critical);
                _config = new LunchBagCameraSettings();
            }

            if (_localSettings.Values.ContainsKey("CognitiveVisionAPIKey"))
            {
                _config.CognitiveVisionAPIKey = _localSettings.Values["CognitiveVisionAPIKey"].ToString();
            }

            if (string.IsNullOrEmpty(_config.EventId))
                _logging.LogMessage("Paramater EventId not found and is required to run the app. Please set a desired property 'EventId'.", LoggingLevel.Warning);

            if (string.IsNullOrEmpty(_config.CognitiveVisionAPIKey))
                _logging.LogMessage($"Paramater CognitiveVisionAPIKey not found and is required to run the app. Please set a desired property 'CognitiveVisionAPIKey'.", LoggingLevel.Warning);

            if (string.IsNullOrEmpty(_config.CameraResolution))
            {
                _config.CameraResolution = "1920x1080";
                _logging.LogMessage($"No Camera Resolution found, assuming resolution {_config.CameraResolution}. Please set a desired property 'CameraResolution'.", LoggingLevel.Warning);
            }

            if (_config.IntervalPhoto == 0)
            {
                _config.IntervalPhoto = _config.IntervalPhoto < 2 ? 2 : _config.IntervalPhoto;
                _logging.LogMessage($"The minimum value for IntervalPhoto is 2. Setting the value at 2. Please set a desired property 'IntervalPhoto' equal or above 2.", LoggingLevel.Warning);
            }
        }

        private async Task ReceiveConfiguration(TwinCollection desiredProperties, object userContext)
        {
            //Reload configuration
            await ReloadApplication();
        }

        private async Task<bool> ReceiveSubscription(string subscriptionKey)
        {
            if (!string.IsNullOrEmpty(subscriptionKey))
            {
                _logging.LogMessage("Updating CognitiveVisionAPIKey", LoggingLevel.Verbose);
                _localSettings.Values["CognitiveVisionAPIKey"] = subscriptionKey;
                await ReloadApplication();
                return true;
            }
            else
            {
                _logging.LogMessage("No CognitiveVisionAPIKey was sent", LoggingLevel.Error);
                return false;
            }
        }

        private async Task ReloadApplication()
        {
            //Request program interruption
            _isInterruptRequested = true;

            //Wait for program interruption
            while (_isRunning)
            {
                await Task.Delay(1000);
            }

            _isInterruptRequested = false;
        }
    }
}
