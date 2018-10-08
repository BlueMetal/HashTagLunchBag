using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LunchBag.Common.Models;
using LunchBag.WebPortal.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;

namespace LunchBag.WebPortal.Api
{
    [Route("Notes")]
    [Authorize]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly NoteDataManager _noteDataManager;

        public NotesController(NoteDataManager noteDataManager)
        {
            _noteDataManager = noteDataManager;
        }

        [HttpGet("{eventId}")]
        [Produces(typeof(IEnumerable<NoteModel>))]
        public async Task<IActionResult> GetNotes(string eventId)
        {
            IEnumerable<NoteModel> notes = await _noteDataManager.GetNotesFromEvent(eventId);
            return Ok(notes);
        }

        [HttpGet("Templates/{eventId}")]
        [Produces(typeof(IEnumerable<NoteTemplateModel>))]
        public async Task<IActionResult> GetNoteTemplates(string eventId)
        {
            IEnumerable<NoteTemplateModel> notes = await _noteDataManager.GetNoteTemplates(eventId);
            return Ok(notes);
        }

        [HttpPost("")]
        [Produces(typeof(string))]
        public async Task<IActionResult> CreateNote([FromBody]NoteModel note)
        {
            string result = await _noteDataManager.CreateNote(note);
            return Ok(result);
        }

        [HttpPost("Templates")]
        [Produces(typeof(string))]
        public async Task<IActionResult> CreateNoteTemplate([FromBody]NoteTemplateModel noteTemplate)
        {
            string result = await _noteDataManager.CreateNoteTemplate(noteTemplate);
            return Ok(result);
        }

        [HttpDelete("Templates/{noteTemplateId}")]
        public async Task<IActionResult> DeleteNoteTemplate(string noteTemplateId)
        {
            bool result = await _noteDataManager.DeleteNoteTemplate(noteTemplateId);
            return Ok(result);
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteAllNotes(string eventId)
        {
            var result = await _noteDataManager.DeleteAllNotes(eventId);
            return Ok(result);
        }
    }
}
