using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LunchBag.Common.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LunchBag.AdminPortal.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly ILogger<RegistrationsController> _logger;
        private readonly IRegistrationRestService _registrationRestService;

        public RegistrationsController(IRegistrationRestService registrationRestService, ILogger<RegistrationsController> logger)
        {
            _logger = logger;
            _registrationRestService = registrationRestService;
        }

        [HttpGet]
        public async Task<FileResult> GetRegistrations()
        {
            byte[] file = await _registrationRestService.GetRegistrationsExtract();
            if (file != null && file.Length != 0)
                return File(file, "text/csv", $"extract_registrations_{DateTime.UtcNow.ToString("yyyy_MM_dd")}.csv");
            return null;
        }
    }
}