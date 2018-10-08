using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.Common.Models
{
    public class EventPayPalApiModel
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public PayPalMerchantInfo MerchantInfo { get; set; }
    }

    public class PayPalMerchantInfo
    {
        public string Email { get; set; }
        public string BusinessName { get; set; }
        public string DonationName { get; set; }
        public string Currency { get; set; }
        public string ThanksNote { get; set; }
    }
}
