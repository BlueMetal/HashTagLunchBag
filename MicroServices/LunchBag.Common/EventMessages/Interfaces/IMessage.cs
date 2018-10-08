using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.Common.EventMessages
{
    public interface IMessage
    {
        string EventId { get; set; }
    }
}
