using LunchBag.Common.Managers;
using LunchBag.Common.Models;
using LunchBag.WebPortal.Api.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.Api.Helpers
{
    public class NoteDataManager
    {
        private readonly WebPortalApiConfig _config;
        private readonly Dictionary<string, IEnumerable<NoteModel>> _dictEvents;
        private readonly Dictionary<string, IEnumerable<NoteTemplateModel>> _dictNoteTemplates;
        private readonly ICosmosDBRepository<NoteModel> _repoNotes;
        private readonly ICosmosDBRepository<NoteTemplateModel> _repoNoteTemplates;
        private readonly ILogger<NoteDataManager> _logger;

        public NoteDataManager(IOptions<WebPortalApiConfig> config,
            ICosmosDBRepository<NoteModel> repoNotes,
            ICosmosDBRepository<NoteTemplateModel> repoNoteTemplates,
            ILogger<NoteDataManager> logger)
        {
            _logger = logger;
            _config = config.Value;
            _repoNotes = repoNotes;
            _repoNoteTemplates = repoNoteTemplates;
            _dictEvents = new Dictionary<string, IEnumerable<NoteModel>>();
            _dictNoteTemplates = new Dictionary<string, IEnumerable<NoteTemplateModel>>();
        }

        public async Task<IEnumerable<NoteModel>> GetNotesFromEvent(string eventId, bool forceRefresh = false)
        {
            IEnumerable<NoteModel> notes = null;
            if (!_dictEvents.ContainsKey(eventId) || !_config.UseCache || (_config.UseCache && forceRefresh))
            {
                notes = await _repoNotes.GetItemsAsyncOrderByDescending(p => p.EventId == eventId, p => p.Date, _config.MaxNoteItems);
                if (notes == null)
                    _logger.LogDebug($"GetNotesFromEvent: No notes found for event {eventId}.");
                _dictEvents[eventId] = notes;
            }
            else
                notes = _dictEvents[eventId];
            return notes;
        }

        public async Task<IEnumerable<NoteTemplateModel>> GetNoteTemplates(string eventId, bool forceRefresh = false)
        {
            IEnumerable<NoteTemplateModel> noteTemplates = null;
            if (!_dictNoteTemplates.ContainsKey(eventId) || !_config.UseCache || (_config.UseCache && forceRefresh))
            {
                noteTemplates = await _repoNoteTemplates.GetItemsAsync(p => p.EventId == eventId);
                if (noteTemplates == null)
                    _logger.LogDebug($"GetNoteTemplates: No note templates found.");
                _dictNoteTemplates[eventId] = noteTemplates;
            }
            else
                noteTemplates = _dictNoteTemplates[eventId];
            return noteTemplates;
        }

        public async Task<string> CreateNote(NoteModel note)
        {
            string result = await _repoNotes.CreateItemAsync(note);
            if (_config.UseCache)
                await GetNotesFromEvent(note.EventId, true);
            return result;
        }

        public async Task<string> CreateNoteTemplate(NoteTemplateModel noteTemplate)
        {
            string result = await _repoNoteTemplates.CreateItemAsync(noteTemplate);
            if (_config.UseCache)
                await GetNotesFromEvent(noteTemplate.EventId, true);
            return result;
        }

        public async Task<bool> DeleteNoteTemplate(string noteTemplateid)
        {
            bool result = await _repoNoteTemplates.DeleteItemAsync(noteTemplateid);
            if (result)
                _dictNoteTemplates?.Remove(noteTemplateid);
            return result;
        }

        public async Task<bool> DeleteAllNotes(string eventId)
        {
            bool result = await _repoNotes.DeleteItemsAsync(p => p.EventId == eventId);
            if (result)
                _dictEvents[eventId] = null;
            return result;
        }
    }
}
