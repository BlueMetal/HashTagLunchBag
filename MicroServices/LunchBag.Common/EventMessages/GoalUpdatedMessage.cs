using System;
using Newtonsoft.Json;

namespace LunchBag.Common.EventMessages
{
    public class GoalUpdatedMessage : IGoalUpdatedMessage
    {
        public string LocationId { get; set; }
        public string EventId { get; set; }
        public int GoalStatus { get; set; }
    }
}
