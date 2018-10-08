using System;
using Newtonsoft.Json;

namespace LunchBag.Common.EventMessages
{
    public class DeliveryUpdatedMessage : IDeliveryUpdatedMessage
    {
        public string LocationId { get; set; }
        public string EventId { get; set; }
        public string DeliveryId { get; set; }
        public string RouteId { get; set; }
        public int Status { get; set; }
    }
}
