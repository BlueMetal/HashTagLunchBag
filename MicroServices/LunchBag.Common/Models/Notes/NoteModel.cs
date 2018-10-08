using LunchBag.Common.EventMessages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.Common.Models
{
    public class NoteModel : IEntityModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string EventId { get; set; }
        public string Note { get; set; }
        public string Sentiment { get; set; }
        public DateTime Date { get; set; }
        public string ETag { get; set; }
    }
}
