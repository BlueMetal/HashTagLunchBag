using LunchBag.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.WebPortal.Api.Models.SignalRMessages
{
    public class ViewUpdatedSignal
    {
        public List<string> Views { get; set; }

        public int CyclingInterval { get; set; }
    }
}
