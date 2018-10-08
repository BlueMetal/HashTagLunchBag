using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.Api.Models
{
    public class ClientEventModel
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public List<EventLocationModel> EventLocations { get; set; }
        public List<EventSentimentModel> EventSentiments { get; set; }
        public bool IsEventActive { get; set; }
        public string LastNote { get; set; }
        public int Goal
        {
            get
            {
                int goal = 0;
                if (EventLocations != null)
                    foreach (var eventLocation in EventLocations)
                        goal += eventLocation.Goal;
                return goal;
            }
        }
        public EventViewModel EventView { get; set; }
    }
}
