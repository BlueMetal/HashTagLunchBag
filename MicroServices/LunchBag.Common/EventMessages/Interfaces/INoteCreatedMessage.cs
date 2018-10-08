namespace LunchBag.Common.EventMessages
{
    public interface INoteCreatedMessage : IMessage
    {
        string Note { get; set; }
    }
}
