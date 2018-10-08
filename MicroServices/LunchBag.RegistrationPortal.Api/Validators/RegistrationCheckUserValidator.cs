using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LunchBag.RegistrationPortal.Api.Models.ApiMessages;

namespace LunchBag.RegistrationPortal.Api.Validators
{
    public class RegistrationCheckUserValidator : AbstractValidator<string>
    {
        public RegistrationCheckUserValidator()
        {
            RuleFor(x => x).NotEmpty().EmailAddress().WithMessage("Please specify a valid address");
        }
    }
}
