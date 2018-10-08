using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Config
{
    public class PayPalApiConfig
    {
        public string Url { get; set; }

        // the majority of pay pal settings are managed per-event in the
        //  event model and not at an application level
    }
}
