using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.TransportService.Config
{
    public class MessageServiceOptions
    {
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int PushToQueueTimeToLive { get; set; }
    }
}
