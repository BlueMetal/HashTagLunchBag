namespace LunchBag.Common.EventMessages
{
    public interface IDeliveryUpdatedMessage : IMessage
    {
        string LocationId { get; set; }
        string DeliveryId { get; set; }
        string RouteId { get; set; }
        int Status { get; set; }
    }
}
