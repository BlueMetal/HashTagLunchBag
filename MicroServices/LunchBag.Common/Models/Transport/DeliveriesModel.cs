using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models.Transport
{
    public class DeliveriesModel
    {
        public int Count { get; set; }
        public int TotalMiles { get; set; }
        public int TotalLunches { get; set; }
        public IEnumerable<DeliveryModel> Deliveries { get; set; }
    }
}
