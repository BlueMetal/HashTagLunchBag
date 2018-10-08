using FluentValidation;
using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.Api.Validators
{
    public class EventModelValidator : AbstractValidator<EventModel>
    {
        public EventModelValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Please specify a valid event ID");
            RuleFor(x => x.EventName).NotEmpty().WithMessage("Please specify a valid event Name");
        }
    }
}
