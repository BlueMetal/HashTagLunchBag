using LunchBag.Common.Models;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.EventMessages
{
    public class SentimentsUpdatedMessage : ISentimentsUpdatedMessage
    {
        public string EventId { get; set; }
        public List<EventSentimentModel> Sentiments { get; set; }
    }
}
