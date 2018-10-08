using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LunchBag.RegistrationPortal.Api.Models.ApiMessages;

namespace LunchBag.RegistrationPortal.Api.Validators
{
    public class RegistrationCreatedMessageValidator : AbstractValidator<RegistrationCreatedMessage>
    {
        public RegistrationCreatedMessageValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage("Please specify a valid event ID");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Please specify a valid location ID");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Please specify a valid address");
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).WithMessage("Please specify a first name");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50).WithMessage("Please specify a last name");
            RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(12).WithMessage("Please specify a zip code");
        }
    }
}
