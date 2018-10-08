using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Media.MediaProperties;
using Ms.IoT.LunchBag.Camera.Models;
using System.IO;
using Windows.Storage.Streams;
using Windows.Foundation.Diagnostics;

namespace Ms.IoT.LunchBag.Camera.Helper
{
    internal class CameraService : IDisposable
    {
        private readonly LoggingChannel _logging;
        private MediaCapture _mediaCapture;

        internal bool IsInitialized { get; private set; }

        internal CameraService(LoggingChannel logging)
        {
            //Logs
            _logging = logging;
        }

        internal async Task Init(int cameraIndex, string cameraResolution)
        {
            if (IsInitialized)
                return;

            _logging.LogMessage("Initializing Camera Service.", LoggingLevel.Verbose);

            // Create MediaCapture and its settings
            _mediaCapture = new MediaCapture();

            // Register for a notification when something goes wrong
            _mediaCapture.Failed += MediaCapture_Failed;

            var cameraDevice = await GetCameraDeviceByIdAsync(cameraIndex);
            if (cameraDevice == null)
            {
                _logging.LogMessage("No camera device found!", LoggingLevel.Error);
                return;
            }

            // Initialize MediaCapture
            try
            {
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };
                await _mediaCapture.InitializeAsync(settings);

                StreamPropertiesModel[] supportedResolutions = GetStreamProperties();

                StreamPropertiesModel configuredResolution = supportedResolutions.FirstOrDefault(p => p.Resolution == cameraResolution);
                if (configuredResolution == null)
                    configuredResolution = supportedResolutions.FirstOrDefault(p => p.Resolution == "1920x1080");
                if (configuredResolution == null)
                    configuredResolution = supportedResolutions.FirstOrDefault(p => p.Resolution == "1280x720");
                if (configuredResolution == null)
                    configuredResolution = supportedResolutions.FirstOrDefault();

                await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, configuredResolution.EncodingProperties);

                IsInitialized = true;
            }
            catch (UnauthorizedAccessException)
            {
                _logging.LogMessage("CameraService - Init: The app was denied access to the camera", LoggingLevel.Critical);
                return;
            }
            return;
        }

        internal StreamPropertiesModel[] GetStreamProperties()
        {
            IEnumerable<StreamPropertiesModel> supportedResolutions = _mediaCapture.VideoDeviceController
                    .GetAvailableMediaStreamProperties(MediaStreamType.Photo)
                    .Select(x => new StreamPropertiesModel(x))
                    .OrderByDescending(x => x.Height * x.Width)
                    .ThenByDescending(x => x.FrameRate)
                    .GroupBy(p => p.Resolution)
                    .Select(p => new StreamPropertiesModel(p.First().EncodingProperties));

            return supportedResolutions.ToArray();
        }

        private async Task<DeviceInformation> GetCameraDeviceByIdAsync(int index)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            if (allVideoDevices == null || allVideoDevices.Count == 0)
                return null;

            //In case the index is not found, resetting to 0
            if (index > allVideoDevices.Count - 1)
                index = 0;

            DeviceInformation desiredDevice = allVideoDevices[index];

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice;
        }

        internal async Task<StorageFile> TakePhotoToStorage()
        {
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
            StorageFile file = null;
            try
            {
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync("temp.jpg", CreationCollisionOption.GenerateUniqueName);
                await _mediaCapture.CapturePhotoToStorageFileAsync(imgFormat, file);
            }
            catch (Exception e)
            {
                _logging.LogMessage(e.Message, LoggingLevel.Critical);
            }
            return file;
        }

        internal async Task TakePhotoToStream(InMemoryRandomAccessStream stream)
        {
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
            try
            {
                // take photo
                if(_mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Shutdown)
                    await _mediaCapture.CapturePhotoToStreamAsync(imgFormat, stream);
            }
            catch (Exception e) {
                _logging.LogMessage(e.Message, LoggingLevel.Critical);
            }
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            _logging.LogMessage($"MediaCapture_Failed: {errorEventArgs.Message}", LoggingLevel.Error);
        }

        public void Dispose()
        {
            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
                IsInitialized = false;
            }
        }
    }
}
