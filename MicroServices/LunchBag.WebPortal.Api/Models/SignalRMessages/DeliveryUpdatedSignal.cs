using Newtonsoft.Json;

namespace LunchBag.WebPortal.Api.Models.SignalRMessages
{
    public class DeliveryUpdatedSignal
    {
        public string LocationId { get; set; }
        public string DeliveryId { get; set; }
        public string RouteId { get; set; }
        public int Status { get; set; }
    }
}
