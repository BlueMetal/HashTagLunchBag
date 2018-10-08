using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LunchBag.Common.EventMessages;
using LunchBag.Common.Models;
using LunchBag.WebPortal.Api.Helpers;
using LunchBag.WebPortal.Api.Models.SignalRMessages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;

namespace LunchBag.WebPortal.Api
{
    [Route("Signals")]
    [Authorize]
    [ApiController]
    public class SignalRController : ControllerBase
    {
        private readonly EventDataManager _eventDataManager;
        private readonly IHubContext<SignalRHub> _hub;

        public SignalRController(EventDataManager eventDataManager,
            IHubContext<SignalRHub> hub)
        {
            _hub = hub;
            _eventDataManager = eventDataManager;
        }

        [HttpPost("Goal")]
        public async Task<IActionResult> SendGoalUpdate([FromBody]GoalUpdatedMessage message)
        {
            await _hub.Clients.Group(message.EventId).SendAsync("ReceiveGoalUpdate", new GoalUpdatedSignal()
            {
                LocationId = message.LocationId,
                GoalStatus = message.GoalStatus
            });
            return Ok();
        }

        [HttpPost("View")]
        public async Task<IActionResult> SendViewUpdate([FromBody]ViewUpdatedMessage message)
        {
            await _hub.Clients.Group($"{message.EventId}_{message.ViewId}").SendAsync("ReceiveViewUpdate", new ViewUpdatedSignal()
            {
                Views = message.Views,
                CyclingInterval = message.CyclingInterval
            });
            return Ok();
        }

        [HttpPost("Note")]
        public async Task<IActionResult> SendNoteCreated([FromBody]NoteCreatedMessage message)
        {
            await _hub.Clients.Group(message.EventId).SendAsync("ReceiveNote", new NoteCreatedSignal()
            {
                Note = message.Note
            });
            return Ok();
        }

        [HttpPost("Sentiment")]
        public async Task<IActionResult> SendSentimentUpdated([FromBody]SentimentsUpdatedMessage message)
        {
            await _hub.Clients.Group(message.EventId).SendAsync("ReceiveSentimentUpdate", message.Sentiments);
            return Ok();
        }

        [HttpPost("ActiveState")]
        public async Task<IActionResult> SendEventActiveChanged([FromBody]EventActiveStateChangedMessage message)
        {
            await _hub.Clients.Group(message.EventId).SendAsync("ReceiveActiveStateChange", message.EventActiveState);
            return Ok();
        }

        [HttpPost("Deliveries")]
        public async Task<IActionResult> SendDeliveryUpdate([FromBody]DeliveryUpdatedMessage message)
        {
            await _hub.Clients.Group(message.EventId).SendAsync("ReceiveDeliveryUpdate", new DeliveryUpdatedSignal()
            {
                LocationId = message.LocationId,
                DeliveryId = message.DeliveryId,
                RouteId = message.RouteId,
                Status = message.Status
            });
            return Ok();
        }
    }
}
