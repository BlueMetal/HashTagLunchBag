using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LunchBag.Common.Models
{
    public class EventModel : IEntityModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string EventName { get; set; }
        public EventPayPalApiModel PayPalApi { get; set; }
        public List<EventLocationModel> EventLocations { get; set; }
        public List<EventSentimentModel> EventSentiments { get; set; }
        public List<EventViewModel> EventViews { get; set; }
        public bool IsEventActive { get; set; }
        public string LastNote { get; set; }
        public DateTime Date { get; set; }
        public string ETag { get; set; }
    }
}
