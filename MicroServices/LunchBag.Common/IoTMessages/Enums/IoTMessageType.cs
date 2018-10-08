using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LunchBag.Common.IoTMessages
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IoTMessageType
    {
        Unknown = 0,
        Button = 1,
        Note = 2
    }
}
