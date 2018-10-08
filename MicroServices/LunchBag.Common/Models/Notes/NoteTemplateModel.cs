using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunchBag.Common.Models
{
    public class NoteTemplateModel : IEntityModel
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string ETag { get; set; }

        public string EventId { get; set; }
        public string Note { get; set; }
        public string Sentiment { get; set; }
    }
}
