using LunchBag.Common.EventMessages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.Common.Models
{
    public class EventLocationModel
    {
        public string Id { get; set; }
        public string LocationName { get; set; }
        public int GoalStatus { get; set; }
        public int Goal { get; set; }
    }
}
