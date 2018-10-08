using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport.Bing
{
    public class BingSnapBody
    {
        public List<LocationModel> points { get; set; } = new List<LocationModel>();
        public bool interpolate { get; set; } = true;
        public string travelMode { get; set; } = "driving";
        public bool includeSpeedLimit { get; set; } = false;
        public bool includeTruckSpeedLimit { get; set; } = false;
        public string speedUnit { get; set; } = "MPH";
    }
}
