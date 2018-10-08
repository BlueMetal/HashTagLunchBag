using LunchBag.Common.Models;
using System;
using System.Collections.Generic;

namespace LunchBag.AdminPortal.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {

        }

        public IEnumerable<EventModel> Events { get; set; }
    }
}