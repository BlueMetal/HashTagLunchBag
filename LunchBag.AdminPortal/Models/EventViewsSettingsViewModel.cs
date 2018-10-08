using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.AdminPortal.Models
{
    public class EventSentimentsSettingsViewModel
    {
        public string EventId { get; set; }
        public List<EventSentimentModel> EventSentiments { get; set; }
    }
}
