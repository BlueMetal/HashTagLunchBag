using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport
{
    public class GeocodeModel
    {
        public LocationModel location { get; set; }
        public string formattedAddress { get; set; }
        public string confidence { get; set; }
    }
}
