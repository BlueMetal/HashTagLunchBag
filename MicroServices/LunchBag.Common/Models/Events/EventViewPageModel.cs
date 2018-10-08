using LunchBag.Common.EventMessages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.Common.Models
{
    public class EventViewPageModel
    {
        public string ViewId { get; set; }
        public List<string> Views { get; set; }
        public int CyclingInterval { get; set; }
    }
}
