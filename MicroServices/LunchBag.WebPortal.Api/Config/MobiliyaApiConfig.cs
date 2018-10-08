using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.WebPortal.Api.Config
{
    public class MobiliyaApiConfig
    {
        public bool UseSimulation { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RedisConnection { get; set; }
        public string IdentityService { get; set; }
        public string FleetService { get; set; }
        public string TripService { get; set; }
    }
}
