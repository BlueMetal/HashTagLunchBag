using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using FluentValidation.Results;
using LunchBag.Common.Managers;
using LunchBag.RegistrationPortal.Api.Helpers;
using LunchBag.RegistrationPortal.Api.Models;
using LunchBag.RegistrationPortal.Api.Models.ApiMessages;
using LunchBag.RegistrationPortal.Api.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace LunchBag.RegistrationPortal.Api.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly ICosmosDBRepository<RegistrationModel> _repoRegistrations;
        private readonly PayPalHelper _paypalHelper;

        public RegistrationsController(ICosmosDBRepository<RegistrationModel> repoRegistrations, PayPalHelper paypalHelper)
        {
            _repoRegistrations = repoRegistrations;
            _paypalHelper = paypalHelper;
        }

        [HttpPost]
        [AllowAnonymous]
        [Produces(typeof(HttpStatusCode))]
        public async Task<IActionResult> RegisterNewAttendee([FromBody]RegistrationCreatedMessage message)
        {
            //Formatting
            string email = message.Email.ToLower();
            string firstName = FormatName(message.FirstName);
            string lastName = FormatName(message.LastName);
            string zipCode = message.ZipCode.ToUpper();

            //Test for pre-existence (let the email be the unique key for the registrant)
            RegistrationModel registration = await _repoRegistrations.GetItemAsync(p => p.Email == email);

            if (registration == null)
            {
                //Create
                string newId = await CreateNewRegistration(message.EventId, message.LocationId, email, firstName, lastName, zipCode);
                return !string.IsNullOrEmpty(newId) ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
            }
            else
            {
                //Update
                bool result = await UpdateRegistrationUserInfo(message.EventId, message.LocationId, firstName, lastName, zipCode, registration);
                return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("{email}")]
        [AllowAnonymous]
        [Produces(typeof(RegistrationCheckUserModel))]
        public async Task<IActionResult> CheckExistingUserByEmail(string email)
        {
            RegistrationModel registration = await _repoRegistrations.GetItemAsync(p => p.Email == email.ToLower());

            if (registration != null)
            {
                return Ok(new RegistrationCheckUserModel()
                {
                    UserExist = true,
                    FirstName = registration.FirstName,
                    LastName = registration.LastName
                });
            }

            return Ok(new RegistrationCheckUserModel()
            {
                UserExist = false
            });
        }

        [HttpPost("donations")]
        [AllowAnonymous]
        [Produces(typeof(HttpStatusCode))]
        public async Task<IActionResult> RegisterDonation([FromBody] RegistrationDonationCreationMessage message)
        {
            string email = message.Email.ToLower(); // email address uniquely identifies the user / registration

            RegistrationModel registration = await _repoRegistrations.GetItemAsync(p => p.Email == email);
            if (registration == null)
                return NotFound();

            string donationRef = null;
            if (message.DonationType == DonationType.PayPal)
            {
                // TODO - look at the events repo directly to find paypal information...
                donationRef = await _paypalHelper.SendInvoiceAsync(message.EventId, registration, message.Amount);
                if (donationRef == null)
                    throw new Exception("RegisterDonation: Failed to generate PayPal Invoice");
            }

            bool result = await UpdateRegistrationDonation(message.EventId, message.LocationId, new RegistrationDonationModel()
            {
                Amount = message.Amount,
                DonationType = message.DonationType,
                Reference = donationRef
            },
            registration);

            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpGet("extract")]
        public async Task<byte[]> GetRegistrations()
        {
            byte[] resultFile = null;
            using (var memStream = new MemoryStream())
            {
                using (var sw = new StreamWriter(memStream))
                {
                    var writer = new CsvWriter(sw);
                    var registrations = await _repoRegistrations.GetItemsAsync();

                    foreach (var registration in registrations)
                    {
                        foreach (var eventObj in registration.Events)
                        {
                            writer.WriteField(eventObj.EventId);
                            writer.WriteField(eventObj.LocationId);
                            writer.WriteField(registration.Email);
                            writer.WriteField(registration.FirstName);
                            writer.WriteField(registration.LastName);
                            writer.WriteField(registration.ZipCode);
                            writer.WriteField(eventObj.Date);

                            writer.WriteField(eventObj.Donations.Count);
                            foreach (var donationObj in eventObj.Donations)
                            {
                                writer.WriteField(donationObj.DonationType);
                                writer.WriteField(donationObj.Amount);
                                writer.WriteField(donationObj.Reference ?? string.Empty);
                            }

                            writer.NextRecord();
                        }
                    }
                }
                resultFile = memStream.ToArray();
            }
            if (resultFile != null)
                return resultFile; //File(resultFile, "text/csv", $"extract_registrations_{DateTime.UtcNow.ToString("yyyy_MM_dd")}.csv");
            return null;//StatusCode((int)HttpStatusCode.InternalServerError);
        }

        private async Task<string> CreateNewRegistration(string eventId, string locationId, string email, string firstName, string lastName, string zipCode)
        {
            string newId = await _repoRegistrations.CreateItemAsync(new RegistrationModel()
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ZipCode = zipCode,
                Events = new List<RegistrationEventModel>() { CreateNewRegistrationEvent(eventId, locationId) }
            });
            return newId;
        }

        private async Task<bool> UpdateRegistrationUserInfo(string eventId, string locationId, string firstName, string lastName, string zipCode, RegistrationModel registration)
        {
            try
            {
                if (registration.Events.Find(p => p.EventId == eventId && p.LocationId == locationId) != null
                    && string.Equals(registration.FirstName, firstName)
                    && string.Equals(registration.LastName, lastName)
                    && string.Equals(registration.ZipCode, zipCode))
                {
                    // no change to the registration
                    return true;
                }

                if (registration.Events.Find(p => p.EventId == eventId && p.LocationId == locationId) == null)
                    registration.Events.Add(CreateNewRegistrationEvent(eventId, locationId));

                registration.FirstName = firstName;
                registration.LastName = lastName;
                registration.ZipCode = zipCode;

                return await _repoRegistrations.UpdateItemAsync(registration.Id, registration);
            }
            catch (DocumentClientException dCE)
            {
                if (dCE.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    registration = await _repoRegistrations.GetItemAsync(registration.Id);
                    return await UpdateRegistrationUserInfo(eventId, locationId, firstName, lastName, zipCode, registration);
                }
                throw new Exception(dCE.Message, dCE);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        private async Task<bool> UpdateRegistrationDonation(string eventId, string locationId, RegistrationDonationModel donation, RegistrationModel registration)
        {
            try
            {
                RegistrationEventModel targetEvent = registration.Events.Find(p => p.EventId == eventId && p.LocationId == locationId);
                if (targetEvent == null)
                {
                    registration.Events.Add(CreateNewRegistrationEvent(eventId, locationId));
                    targetEvent = registration.Events.Find(p => p.EventId == eventId);
                }

                targetEvent.Donations.Add(donation);

                return await _repoRegistrations.UpdateItemAsync(registration.Id, registration);
            }
            catch (DocumentClientException dCE)
            {
                if (dCE.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    registration = await _repoRegistrations.GetItemAsync(registration.Id);
                    return await UpdateRegistrationDonation(eventId, locationId, donation, registration);
                }
                throw new Exception(dCE.Message, dCE);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        private RegistrationEventModel CreateNewRegistrationEvent(string eventId, string locationId)
        {
            return new RegistrationEventModel()
            {
                Date = DateTime.UtcNow,
                EventId = eventId,
                LocationId = locationId,
                Donations = new List<RegistrationDonationModel>()
            };
        }

        private string FormatName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return input.First().ToString().ToUpper() + input.ToLower().Substring(1);
        }
    }
}