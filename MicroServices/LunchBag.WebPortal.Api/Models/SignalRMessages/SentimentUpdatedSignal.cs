using LunchBag.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.WebPortal.Api.Models.SignalRMessages
{
    public class SentimentUpdatedSignal
    {
        public string Sentiment { get; set; }

        public int Value { get; set; }
    }
}
