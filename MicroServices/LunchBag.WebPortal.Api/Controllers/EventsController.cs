using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LunchBag.Common.Models;
using LunchBag.WebPortal.Api.Helpers;
using LunchBag.WebPortal.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LunchBag.WebPortal.Api
{
    [Route("Events")]
    [Authorize]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly EventDataManager _eventDataManager;

        public EventsController(EventDataManager eventDataManager)
        {
            _eventDataManager = eventDataManager;
        }
        [HttpGet("{eventId}")]
        [Produces(typeof(EventModel))]
        public async Task<IActionResult> GetEvent(string eventId)
        {
            EventModel vEvent = await _eventDataManager.GetEvent(eventId, false);
            return Ok(vEvent);
        }

        [HttpGet("client/{eventId}")]
        [AllowAnonymous]
        [Produces(typeof(ClientEventModel))]
        public async Task<IActionResult> GetClientEvent(string eventId)
        {
            EventModel vEvent = await _eventDataManager.GetEvent(eventId, true);
            ClientEventModel clientEvent = new ClientEventModel()
            {
                Id = vEvent.Id,
                EventName = vEvent.EventName,
                EventLocations = vEvent.EventLocations,
                EventSentiments = vEvent.EventSentiments,
                EventView = null,
                IsEventActive = vEvent.IsEventActive,
                LastNote = vEvent.LastNote
            };
            return Ok(clientEvent);
        }

        [HttpGet("client/{eventId}/{viewId}")]
        [AllowAnonymous]
        [Produces(typeof(ClientEventModel))]
        public async Task<IActionResult> GetClientEvent(string eventId, string viewId)
        {
            EventModel vEvent = await _eventDataManager.GetEvent(eventId, true);
            ClientEventModel clientEvent = new ClientEventModel()
            {
                Id = vEvent.Id, 
                EventName = vEvent.EventName,
                EventLocations = vEvent.EventLocations,
                EventSentiments = vEvent.EventSentiments,
                EventView = vEvent.EventViews?.Find(p => p.ViewId == viewId),
                IsEventActive = vEvent.IsEventActive,
                LastNote = vEvent.LastNote
            };
            return Ok(clientEvent);
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<EventModel>))]
        public async Task<IActionResult> GetEvents()
        {
            IEnumerable<EventModel> events = await _eventDataManager.GetEvents();
            return Ok(events);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEvent([FromBody]EventModel eventObj)
        {
            await _eventDataManager.UpdateEvent(eventObj);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody]EventModel eventObj)
        {
            var result = await _eventDataManager.CreateEvent(eventObj);
            return Ok(result);
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent(string eventId)
        {
            var result = await _eventDataManager.DeleteEvent(eventId);
            return Ok(result);
        }
    }
}
