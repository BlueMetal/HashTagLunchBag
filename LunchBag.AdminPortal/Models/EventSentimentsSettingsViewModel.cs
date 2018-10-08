using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.AdminPortal.Models
{
    public class EventViewsSettingsViewModel
    {
        public string EventId { get; set; }
        public List<EventViewModel> EventViews { get; set; }
    }
}
