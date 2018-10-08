using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LunchBag.Common.IoTMessages
{
    public class ButtonTriggerIoTMessage : IIoTMessage
    {
        public string UniqueId { get; set; }
        public string LocationId { get; set; }
        public string EventId { get; set; }
        public int Capacity { get; set; }
        [JsonProperty("Type")]
        public IoTMessageType MessageType { get; set; }
    }
}
