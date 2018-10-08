using LunchBag.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Models
{
    public class RegistrationModel : IEntityModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ZipCode { get; set; }
        public List<RegistrationEventModel> Events { get; set; }
        public DateTime Date { get; set; }
        public string ETag { get; set; }
    }
}
