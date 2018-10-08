using LunchBag.Common.Config;
using LunchBag.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public class NoteRestService : RestServiceBase, INoteRestService
    {
        public NoteRestService(IOptions<AzureAdOptions> azureConfig, IOptions<RestServiceOptions> config, ILogger<EventRestService> logger)
            : base(azureConfig, logger, config.Value.RestServiceUrl)
        {
        }

        public async Task<IEnumerable<NoteTemplateModel>> GetNoteTemplates(string eventId)
        {
            RestRequest request = await PrepareQuery("Notes/Templates/{eventId}", Method.GET);
            request.AddUrlSegment("eventId", eventId);
            var queryResult = await _client.ExecuteTaskAsync<IEnumerable<NoteTemplateModel>>(request);
            if (queryResult.IsSuccessful)
                return queryResult.Data;
            else
                _logger.LogError($"GetNoteTemplates: {queryResult.ErrorMessage}");
            return null;
        }

        public async Task<string> CreateNote(NoteModel note)
        {
            RestRequest request = await PrepareQuery("Notes", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(note), ParameterType.RequestBody);
            var result = await _client.ExecuteTaskAsync<string>(request);
            if (!result.IsSuccessful)
                _logger.LogError($"CreateNote: Error while sending a message: {result.StatusCode}");
            return result.Data;
        }

        public async Task<string> CreateNoteTemplate(NoteTemplateModel noteTemplate)
        {
            RestRequest request = await PrepareQuery("Notes/Templates", Method.POST);
            request.AddParameter("application/json", JsonConvert.SerializeObject(noteTemplate), ParameterType.RequestBody);
            var result = await _client.ExecuteTaskAsync<string>(request);
            if (!result.IsSuccessful)
                _logger.LogError($"CreateNoteTemplate: Error while creating a note template: {result.StatusCode}");
            return result.Data;
        }

        public async Task<bool> DeleteNoteTemplate(string noteTemplateId)
        {
            RestRequest request = await PrepareQuery("Notes/Templates/{noteTemplateId}", Method.DELETE);
            request.AddUrlSegment("noteTemplateId", noteTemplateId);
            var result = await _client.ExecuteTaskAsync<bool>(request);
            if (!result.IsSuccessful)
                _logger.LogError($"DeleteNoteTemplate: Error while deleting a note template: {result.StatusCode}");
            return result.Data;
        }

        public async Task<bool> DeleteAllNotes(string eventId)
        {
            RestRequest request = await PrepareQuery("Notes/{eventId}", Method.DELETE);
            request.AddUrlSegment("eventId", eventId);
            var result = await _client.ExecuteTaskAsync<bool>(request);
            if (!result.IsSuccessful)
                _logger.LogError($"DeleteAllNotes: Error while deleting notes related to eventId {eventId}: {result.StatusCode}");
            return result.Data;
        }
    }
}
