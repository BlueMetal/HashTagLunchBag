using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ms.IoT.LunchBag.Camera.Models
{
    public sealed class APISubscriptionKey
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
