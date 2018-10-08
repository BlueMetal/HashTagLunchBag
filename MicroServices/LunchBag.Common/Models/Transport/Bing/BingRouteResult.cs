using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport.Bing
{
    public class BingRouteResult
    {
        public string authenticationResultCode { get; set; }
        public string brandLogoUri { get; set; }
        public string copyright { get; set; }
        public List<BingRouteResourceSet> resourceSets { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string traceId { get; set; }
    }

    public class BingRouteResourceSet
    {
        public int estimatedTotal { get; set; }
        public List<BingRouteResource> resources { get; set; }
    }

    public class BingRouteResource
    {
        public string __type { get; set; }
        public string name { get; set; }
        public string distanceUnit { get; set; }
        public List<BingRouteLeg> routeLegs { get; set; }
        public BingRoutePath routePath { get; set; }
    }

    public class BingRouteLeg
    {
        public decimal travelDistance { get; set; }
    }

    public class BingRoutePath
    {
        public BingRouteLine line { get; set; }
        public List<BingGeneralization> generalizations { get; set; }
    }

    public class BingRouteLine
    {
        public string type { get; set; }
        public List<List<decimal>> coordinates { get; set; }
    }

    public class BingGeneralization
    {
        public double latLongTolerance { get; set; }
        public List<int> pathIndices { get; set; }
    }
}
