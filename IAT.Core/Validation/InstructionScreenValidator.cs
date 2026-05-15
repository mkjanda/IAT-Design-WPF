using FluentValidation;
using IAT.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Validation
{
    public class InstructionScreenValidator : AbstractValidator<InstructionScreen>
    {
        public InstructionScreenValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Instruction screen ID is required")
                .NotEqual(Guid.Empty).WithMessage("Instruction screen ID cannot be empty");
            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Instruction screen text is required");

            When(x => x is KeyedInstructionScreen, () =>
            {
                RuleFor(x => ((KeyedInstructionScreen)x).LeftResponseId).NotNull().NotEmpty().WithMessage("Keyed instruction screen must have left response ID")
                    .NotEqual(Guid.Empty).WithMessage("Keyed instruction screen left response ID cannot be empty");
                RuleFor(x => ((KeyedInstructionScreen)x).RightResponseId).NotNull().NotEmpty().WithMessage("Keyed instruction screen must have right response ID")
                    .NotEqual(Guid.Empty).WithMessage("Keyed instruction screen right response ID cannot be empty");
            });


        }
    }
}
