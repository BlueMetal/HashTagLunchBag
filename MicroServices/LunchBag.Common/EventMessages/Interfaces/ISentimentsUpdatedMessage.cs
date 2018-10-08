using LunchBag.Common.Models;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.EventMessages
{
    public interface ISentimentsUpdatedMessage : IMessage
    {
        List<EventSentimentModel> Sentiments { get; set; }
    }
}
