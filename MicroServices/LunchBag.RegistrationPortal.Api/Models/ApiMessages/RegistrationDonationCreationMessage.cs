using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Models.ApiMessages
{
    public class RegistrationDonationCreationMessage
    {
        [JsonProperty("eventId")]
        public string EventId { get; set; }
        [JsonProperty("locationId")]
        public string LocationId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("type")]
        public DonationType DonationType { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }
    }
}
