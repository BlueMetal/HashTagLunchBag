using LunchBag.Common.Config;
using LunchBag.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public interface INoteRestService
    {
        Task<IEnumerable<NoteTemplateModel>> GetNoteTemplates(string eventId);
        Task<string> CreateNote(NoteModel note);
        Task<string> CreateNoteTemplate(NoteTemplateModel noteTemplate);
        Task<bool> DeleteNoteTemplate(string noteTemplateId);
        Task<bool> DeleteAllNotes(string eventId);
    }
}
