using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Models.ApiMessages
{
    public class RegistrationCreatedMessage
    {
        [JsonProperty("eventId")]
        public string EventId { get; set; }
        [JsonProperty("locationId")]
        public string LocationId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }
    }
}
