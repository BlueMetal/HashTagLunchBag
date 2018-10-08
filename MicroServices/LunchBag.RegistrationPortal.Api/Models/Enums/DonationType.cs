using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DonationType
    {
        Unknown = 0,
        None = 1,
        Cash = 2,
        PayPal = 3,
        CreditCard = 4
    }
}
