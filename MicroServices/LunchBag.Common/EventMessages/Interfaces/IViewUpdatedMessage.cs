using System.Collections.Generic;

namespace LunchBag.Common.EventMessages
{
    public interface IViewUpdatedMessage : IMessage
    {
        string ViewId { get; set; }
        List<string> Views { get; set; }
        int CyclingInterval { get; set; }
    }
}
