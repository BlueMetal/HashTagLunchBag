using LunchBag.Common.Models;
using LunchBag.Common.Models.Transport;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace LunchBag.AdminPortal.Models
{
    public class EventNotesViewModel
    {
        public EventNotesViewModel()
        {

        }

        public string EventId { get; set; }
        public string LastNote { get; set; }

        public IEnumerable<NoteTemplateModel> NoteTemplates { get; set; }

        public IEnumerable<SelectListItem> Sentiments { get; set; }
    }
}