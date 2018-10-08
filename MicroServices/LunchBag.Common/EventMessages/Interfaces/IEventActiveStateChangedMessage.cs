using System.Collections.Generic;

namespace LunchBag.Common.EventMessages
{
    public interface IEventActiveStateChangedMessage : IMessage
    {
        bool EventActiveState { get; set; }
    }
}
