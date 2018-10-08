using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LunchBag.AdminPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LunchBag.Common.Models;
using LunchBag.Common.Managers;
using LunchBag.Common.Models.Transport;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using LunchBag.AdminPortal.Config;

namespace LunchBag.AdminPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AdminService _adminService;
        private readonly ILogger<HomeController> _logger;
        private readonly AdminServiceOptions _config;

        public HomeController(IOptions<AdminServiceOptions> config, AdminService adminService, ILogger<HomeController> logger)
        {
            _adminService = adminService;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<EventModel> models = await _adminService.GetEvents();

            return View(new HomeViewModel()
            {
                Events = models
            });
        }

        public async Task<IActionResult> Edit(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            return View(vEvent);
        }

        public async Task<ActionResult> EditEvent(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            return PartialView("_EditEvent", new EventGeneralSettingsViewModel { 
                EventId = eventId, EventName = vEvent.EventName, IsEventActive = vEvent.IsEventActive
            });
        }

        [HttpPut]
        public async Task<ActionResult> EditEvent(EventGeneralSettingsViewModel pEvent)
        {
            //write code to update student 
            if (!await _adminService.UpdateEventGeneralSettings(pEvent.EventId, pEvent.EventName, pEvent.IsEventActive))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> EditLastNote(EventLastNoteViewModel pEvent)
        {
            //write code to update student 
            if (!await _adminService.UpdateEventLastNoteSettings(pEvent.EventId, pEvent.LastNote))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        public async Task<ActionResult> EditLocations(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            return PartialView("_EditLocations", new EventLocationsSettingsViewModel
            {
                EventId = eventId,
                EventLocations = vEvent.EventLocations
            });
        }

        [HttpPut]
        public async Task<ActionResult> EditLocations(EventLocationsSettingsViewModel pEvent)
        {
            //write code to update student 
            if (!await _adminService.UpdateEventLocations(pEvent.EventId, pEvent.EventLocations))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        public async Task<ActionResult> EditSentiments(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            return PartialView("_EditSentiments", new EventSentimentsSettingsViewModel
            {
                EventId = eventId,
                EventSentiments = vEvent.EventSentiments
            });
        }

        [HttpPut]
        public async Task<ActionResult> EditSentiments(EventSentimentsSettingsViewModel pEvent)
        {
            //write code to update student 
            if (!await _adminService.UpdateEventSentiments(pEvent.EventId, pEvent.EventSentiments))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        public async Task<ActionResult> EditViews(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            return PartialView("_EditViews", new EventViewsSettingsViewModel
            {
                EventId = eventId,
                EventViews = vEvent.EventViews
            });
        }

        [HttpPut]
        public async Task<ActionResult> EditViews(EventViewsSettingsViewModel pEvent)
        {
            //write code to update student 
            if (!await _adminService.UpdateEventViews(pEvent.EventId, pEvent.EventViews))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        public async Task<ActionResult> EditNotes(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            IEnumerable<NoteTemplateModel> templateNotes = await _adminService.GetNoteTemplates(eventId);

            return PartialView("_EditNotes", new EventNotesViewModel()
            {
                EventId = vEvent.Id,
                NoteTemplates = templateNotes,
                LastNote = vEvent.LastNote,
                Sentiments = vEvent.EventSentiments.Select(p => new SelectListItem { Value = p.Name, Text = p.Name })
            });
        }

        public async Task<ActionResult> EditPayPal(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            return PartialView("_EditPayPal", new EventPayPalSettingsViewModel()
            {
                EventId = vEvent.Id,
                ClientId = vEvent.PayPalApi?.ClientId,
                Secret = vEvent.PayPalApi?.Secret,
                BusinessName = vEvent.PayPalApi?.MerchantInfo?.BusinessName,
                DonationName = vEvent.PayPalApi?.MerchantInfo?.DonationName,
                Email = vEvent.PayPalApi?.MerchantInfo?.Email,
                Currency = vEvent.PayPalApi?.MerchantInfo?.Currency,
                ThanksNote = vEvent.PayPalApi?.MerchantInfo?.ThanksNote
            });
        }

        [HttpPut]
        public async Task<ActionResult> EditPayPal(EventPayPalSettingsViewModel pEvent)
        {
            //write code to update student 
            await _adminService.UpdateEventPaypalSettings(pEvent.EventId, 
                pEvent.ClientId, pEvent.Secret, pEvent.Email, pEvent.BusinessName,
                pEvent.DonationName, pEvent.Currency, pEvent.ThanksNote);
            return Ok();
        }

        public async Task<ActionResult> EditDeliveries(string eventId)
        {
            EventModel vEvent = await _adminService.GetEvent(eventId);
            DeliveriesModel deliveries = await _adminService.GetDeliveries(eventId);
            FleetModel fleet = await _adminService.GetFleet(eventId);

            return PartialView("_EditDeliveries", new EventDeliveryViewModel()
            {
                EventId = vEvent.Id,
                Deliveries = deliveries?.Deliveries,
                Drivers = fleet?.Vehicles?.Select(p => new SelectListItem { Value = p.DriverId, Text = $"{p.VehicleName} - {p.DriverName}" }),
                Locations = vEvent.EventLocations.Select(p => new SelectListItem { Value = p.Id, Text = p.LocationName }),
                LinkMobiliyaPortal = _config.LinkMobiliyaPortal
            });
        }

        public IActionResult AddLocation()
        {
            EventLocationModel newLocation = new EventLocationModel { Id = Guid.NewGuid().ToString(), LocationName = "Location", Goal = 2000, GoalStatus = 0 };
            return PartialView("~/Views/Shared/EditorTemplates/EventLocationModel.cshtml", newLocation);
        }

        public IActionResult AddSentiment()
        {
            EventSentimentModel newSentiment = new EventSentimentModel { Name = Guid.NewGuid().ToString(), Value = 0 };
            return PartialView("~/Views/Shared/EditorTemplates/EventSentimentModel.cshtml", newSentiment);
        }

        public IActionResult AddView()
        {
            LunchBag.Common.Models.EventViewModel newView = new LunchBag.Common.Models.EventViewModel { ViewId= Guid.NewGuid().ToString(), Views = new List<string>(), CyclingInterval = 10 };
            return PartialView("~/Views/Shared/EditorTemplates/EventViewModel.cshtml", newView);
        }

        public IActionResult AddViewPage()
        {
            return PartialView("~/Views/Shared/EditorTemplates/EventViewPageModel.cshtml", string.Empty);
        }

        [HttpPost]
        public async Task<IActionResult> AddEvent(EventModel eventModel)
        {
            string id = await _adminService.AddEvent(eventModel);
            if (string.IsNullOrEmpty(id))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddNoteTemplate(NoteTemplateModel noteTemplateModel)
        {
            string id = await _adminService.AddNoteTemplate(noteTemplateModel);
            if (string.IsNullOrEmpty(id))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            noteTemplateModel.Id = id;
            return PartialView("~/Views/Shared/EditorTemplates/NoteTemplateModel.cshtml", noteTemplateModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddDelivery(DeliveryModel delivery)
        {
            string id = await _adminService.AddDelivery(delivery);
            if (string.IsNullOrEmpty(id))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            delivery.Id = id;

            EventModel vEvent = await _adminService.GetEvent(delivery.EventId);
            ViewData["locations"] = vEvent.EventLocations.Select(p => new SelectListItem { Value = p.Id, Text = p.LocationName });

            return PartialView("~/Views/Shared/EditorTemplates/DeliveryModel.cshtml", delivery);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllNotes(string eventId)
        {
            if (!await _adminService.DeleteAllNotes(eventId))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDelivery(string eventId, string deliveryId)
        {
            if (!await _adminService.DeleteDelivery(eventId, deliveryId))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteNoteTemplate(string noteTemplateId)
        {
            if(!await _adminService.DeleteNoteTemplate(noteTemplateId))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEvent(string eventId)
        {
            if (!await _adminService.DeleteEvent(eventId))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
