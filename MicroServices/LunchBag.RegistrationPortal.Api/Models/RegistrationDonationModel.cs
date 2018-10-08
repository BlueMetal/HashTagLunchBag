using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Models
{
    public class RegistrationDonationModel
    {
        public DonationType DonationType { get; set; }
        public string Reference { get; set; }
        public double Amount { get; set; }
    }
}
