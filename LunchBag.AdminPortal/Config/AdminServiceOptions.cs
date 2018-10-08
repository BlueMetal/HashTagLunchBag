using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.AdminPortal.Config
{
    public class AdminServiceOptions
    {
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int PushToQueueTimeToLive { get; set; }

        public string LinkMobiliyaPortal { get; set; }
    }
}
