using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport
{
    public class RouteModel : IEntityModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string RouteId { get; set; }
        public int Count { get; set; }
        public List<LocationModel> Points { get; set; }
        public int RawCount { get; set; }
        public List<LocationModel> RawPoints { get; set; }
        public DateTime Date { get; set; }
        public string ETag { get; set; }
    }
}
