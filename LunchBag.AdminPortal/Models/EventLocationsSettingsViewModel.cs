using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.AdminPortal.Models
{
    public class EventLocationsSettingsViewModel
    {
        public string EventId { get; set; }
        public List<EventLocationModel> EventLocations { get; set; }
    }
}
