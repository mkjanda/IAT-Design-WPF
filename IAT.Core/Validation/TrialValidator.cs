using FluentValidation;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Validation
{
    public class TrialValidator : AbstractValidator<Trial>
    {
        public TrialValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().NotEqual(Guid.Empty).WithMessage("Trial ID is required and cannot be empty");
            RuleFor(x => x.StimulusId).NotNull().NotEmpty().NotEqual(Guid.Empty).WithMessage("Stimulus ID is required and cannot be empty");
            RuleFor(x => x.KeyedDirection)
                .Must(r => r == KeyedDirection.Left || r == KeyedDirection.Right)
                .WithMessage("Correct response must be either Left or Right");
        }
    }
}
