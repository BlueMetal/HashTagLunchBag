using LunchBag.Common.Models;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.EventMessages
{
    public class SentimentUpdatedMessage
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public double Percentage { get; set; }
    }
}
