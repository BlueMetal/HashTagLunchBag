using LunchBag.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Models
{
    public class RegistrationEventModel
    {
        public string EventId { get; set; }
        public string LocationId { get; set; }
        public DateTime Date { get; set; }
        public List<RegistrationDonationModel> Donations { get; set; }
    }
}
