using FluentValidation;
using IAT.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Validation
{
    public class StimulusValidator : AbstractValidator<Stimulus>
    {
        public StimulusValidator() {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Stimulus ID is required")
                .NotEqual(Guid.Empty).WithMessage("Stimulus ID cannot be empty");

            When(x => x is TextStimulus, () =>
            {
                RuleFor(x => ((TextStimulus)x).Text)
                    .NotEmpty().WithMessage("Text stimulus must have non-empty text");
            });
        }
    }
}
