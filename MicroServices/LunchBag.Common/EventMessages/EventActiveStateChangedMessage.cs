using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LunchBag.Common.EventMessages
{
    public class EventActiveStateChangedMessage : IEventActiveStateChangedMessage
    {
        public string EventId { get; set; }
        public bool EventActiveState { get; set; }
    }
}
