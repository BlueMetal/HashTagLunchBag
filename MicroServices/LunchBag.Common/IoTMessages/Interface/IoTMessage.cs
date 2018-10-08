using Newtonsoft.Json;

namespace LunchBag.Common.IoTMessages
{
    public interface IIoTMessage
    {
        [JsonProperty("Type")]
        IoTMessageType MessageType { get; set; }
    }
}
