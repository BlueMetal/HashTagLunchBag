using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace LunchBag.AdminPortal.Models
{
    public class EventDeliveryViewModel
    {
        public EventDeliveryViewModel()
        {

        }

        public string EventId { get; set; }

        public string LinkMobiliyaPortal { get; set; }

        public IEnumerable<SelectListItem> Locations { get; set; }

        public IEnumerable<SelectListItem> Drivers { get; set; }

        public IEnumerable<DeliveryModel> Deliveries { get; set; }
    }
}