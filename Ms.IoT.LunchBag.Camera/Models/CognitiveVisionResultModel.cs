using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ms.IoT.LunchBag.Camera.Models
{
    public sealed class CognitiveVisionResultModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("recognitionResult")]
        public RecognitionResultModel RecognitionResult { get; set; }
    }

    public sealed class RecognitionResultModel
    {
        [JsonProperty("lines")]
        public IEnumerable<RecognitionResultLineModel> Lines { get; set; }
    }

    public sealed class RecognitionResultLineModel
    {
        [JsonProperty("boundingBox")]
        public IEnumerable<uint> BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("words")]
        public IEnumerable<RecognitionResultWordModel> Words { get; set; }
    }

    public sealed class RecognitionResultWordModel
    {
        [JsonProperty("boundingBox")]
        public IEnumerable<uint> BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
