using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport.Bing
{
    public class BingGeocodeResult
    {
        public string authenticationResultCode { get; set; }
        public string brandLogoUri { get; set; }
        public string copyright { get; set; }
        public List<BingGeocodeResourceSet> resourceSets { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string traceId { get; set; }
    }

    public class BingGeocodeResourceSet
    {
        public int estimatedTotal { get; set; }
        public List<BingGeocodeResource> resources { get; set; }
    }

    public class BingGeocodeResource
    {
        public string __type { get; set; }
        public string name { get; set; }
        public BingGeocodePoint point { get; set; }
        public BingGeocodeAddress address { get; set; }
        public string confidence { get; set; }
    }

    public class BingGeocodePoint
    {
        public string type { get; set; }
        public List<decimal> coordinates { get; set; }
    }

    public class BingGeocodeAddress
    {
        public string addressLine { get; set; }
        public string adminDistrict { get; set; }
        public string countryRegion { get; set; }
        public string locality { get; set; }
        public string postalCode { get; set; }
        public string formattedAddress { get; set; }
    }
}
