using LunchBag.Common.EventMessages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.Common.Models
{
    public class EventSentimentModel
    {
        public string Name { get; set; }
        public double Percentage { get; set; }
        public int Value { get; set; }
    }
}
