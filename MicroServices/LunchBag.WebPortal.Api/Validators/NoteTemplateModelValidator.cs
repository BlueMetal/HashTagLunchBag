using FluentValidation;
using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchBag.WebPortal.Api.Validators
{
    public class NoteTemplateModelValidator : AbstractValidator<NoteTemplateModel>
    {
        public NoteTemplateModelValidator()
        {
            RuleFor(x => x.EventId).NotEmpty().WithMessage("Please specify a valid event ID");
            RuleFor(x => x.Note).NotEmpty().WithMessage("Please specify a note");
            RuleFor(x => x.Sentiment).NotEmpty().WithMessage("Please specify a sentiment");
        }
    }
}
