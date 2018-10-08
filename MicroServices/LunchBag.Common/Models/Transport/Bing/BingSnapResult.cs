using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport.Bing
{
    public class BingSnapResult
    {
        public string authenticationResultCode { get; set; }
        public string brandLogoUri { get; set; }
        public string copyright { get; set; }
        public List<BingSnapResourceSet> resourceSets { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string traceId { get; set; }
    }

    public class BingSnapResourceSet
    {
        public int estimatedTotal { get; set; }
        public List<BingSnapResource> resources { get; set; }
    }

    public class BingSnapResource
    {
        public string __type { get; set; }
        public List<BingSnappedPoint> snappedPoints { get; set; }
    }

    public class BingSnappedPoint
    {
        public LocationModel coordinate { get; set; }
        public int index { get; set; }
        public string name { get; set; }
        public int speedLimit { get; set; }
        public string speedUnit { get; set; }
        public int truckSpeedLimit { get; set; }
    }
}
