using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LunchBag.Common.EventMessages
{
    public class ViewUpdatedMessage : IViewUpdatedMessage
    {
        public string EventId { get; set; }
        public string ViewId { get; set; }
        public List<string> Views { get; set; }
        public int CyclingInterval { get; set; }
    }
}
