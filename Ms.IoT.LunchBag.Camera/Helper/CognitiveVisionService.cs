using Ms.IoT.LunchBag.Camera.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Ms.IoT.LunchBag.Camera.Helper
{
    internal class CognitiveVisionService
    {
        private readonly LoggingChannel _logging;
        private string _subscriptionKey;
        private const string uriBase = "https://eastus.api.cognitive.microsoft.com/vision/v2.0/recognizeText";

        internal CognitiveVisionService(LoggingChannel logging)
        {
            //Logs
            _logging = logging;
        }

        internal void Init(string subscriptionKey)
        {
            _logging.LogMessage("Initializing Cognitive Vision Service.", LoggingLevel.Verbose);
            if (string.IsNullOrEmpty(subscriptionKey))
                _logging.LogMessage("No subscription key for CognitiveVision service found.", LoggingLevel.Error);
            _subscriptionKey = subscriptionKey;
        }

        internal async Task<string> ProcessPhotoFromFile(string filePath)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filePath);
            return await ProcessPhotoFromFile(file);
        }

        internal async Task<string> ProcessPhotoFromFile(StorageFile file)
        {
            byte[] data = null;
            using (var memoryStream = new MemoryStream())
            {
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    stream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
            }
            if (data != null && data.Length != 0)
                return await ReadHandwrittenText(data);
            return string.Empty;
        }

        internal async Task<string> ProcessPhotoFromStream(InMemoryRandomAccessStream inMemoryRandomAccessStream)
        {
            byte[] data = null;
            using (Stream stream = inMemoryRandomAccessStream.AsStreamForRead())
            {
                using (var memoryStream = new MemoryStream())
                {
                    inMemoryRandomAccessStream.Seek(0);
                    stream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
            }
            if (data != null && data.Length != 0)
                return await ReadHandwrittenText(data);
            return string.Empty;
        }

        /// <summary>
        /// Gets the handwritten text from the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with handwritten text.</param>
        private async Task<string> ReadHandwrittenText(byte[] byteData)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", _subscriptionKey);

                // Request parameter.
                // Note: The request parameter changed for APIv2.
                // For APIv1, it is "handwriting=true".
                string requestParameters = "mode=Handwritten";

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Two REST API calls are required to extract handwritten text.
                // One call to submit the image for processing, the other call
                // to retrieve the text found in the image.
                // operationLocation stores the REST API location to call to
                // retrieve the text.
                string operationLocation;

                // Request body.
                // Posts a locally stored JPEG image.
                //byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // The first REST call starts the async process to analyze the
                    // written text in the image.
                    response = await client.PostAsync(uri, content);
                }

                // The response contains the URI to retrieve the result of the process.
                if (response.IsSuccessStatusCode)
                    operationLocation =
                        response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    // Display the JSON error data.
                    string errorString = await response.Content.ReadAsStringAsync();
                    _logging.LogMessage($"ReadHandwrittenText: {JToken.Parse(errorString).ToString()}", LoggingLevel.Error);
                    return string.Empty;
                }

                // The second REST call retrieves the text written in the image.
                //
                // Note: The response may not be immediately available. Handwriting
                // recognition is an async operation that can take a variable amount
                // of time depending on the length of the handwritten text. You may
                // need to wait or retry this operation.
                //
                // This example checks once per second for ten seconds.
                string contentString;
                CognitiveVisionResultModel result = null;
                int i = 0;
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<CognitiveVisionResultModel>(contentString);
                    ++i;
                }
                while (i < 10 && result.Status != "Succeeded");

                if (i == 10 && result.Status != "Succeeded")
                {
                    _logging.LogMessage("ReadHandwrittenText: Timeout error.", LoggingLevel.Error);
                    return string.Empty;
                }

                // Display the JSON response.
                _logging.LogMessage($"ReadHandwrittenText: {JToken.Parse(contentString).ToString()}", LoggingLevel.Verbose);
                if(result.RecognitionResult != null && result.RecognitionResult.Lines != null && result.RecognitionResult.Lines.Count() > 0)
                    return string.Join(" ", result.RecognitionResult.Lines.Select(p => p.Text));
                return string.Empty;
            }
            catch (Exception e)
            {
                _logging.LogMessage($"ReadHandwrittenText: {e.Message}", LoggingLevel.Critical);
                return string.Empty;
            }
        }
    }
}
