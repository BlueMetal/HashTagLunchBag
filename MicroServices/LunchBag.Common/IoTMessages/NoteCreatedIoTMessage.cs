using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LunchBag.Common.IoTMessages
{
    public class NoteCreatedIoTMessage : IIoTMessage
    {
        public string EventId { get; set; }
        public string Note { get; set; }
        [JsonProperty("Type")]
        public IoTMessageType MessageType { get; set; }
    }
}
