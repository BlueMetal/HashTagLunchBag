using FluentValidation;
using LunchBag.RegistrationPortal.Api.Models.ApiMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.RegistrationPortal.Api.Validators
{
    public class RegistrationDonationCreationMessageValidator : AbstractValidator<RegistrationDonationCreationMessage>
    {
        public RegistrationDonationCreationMessageValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage("Please specify a valid event ID");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Please specify a valid location ID");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Please specify a valid address");
            RuleFor(x => x.Amount).NotEmpty().WithMessage("Please specify a valid amount");
            RuleFor(x => x.DonationType).NotEqual(Models.DonationType.Unknown).WithMessage("Please specify a donation type");
        }
    }
}
