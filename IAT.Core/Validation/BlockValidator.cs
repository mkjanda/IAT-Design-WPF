using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Domain;

namespace IAT.Core.Validation
{
    public class BlockValidator : AbstractValidator<Block>
    {
        public BlockValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Block ID is required")
                .NotEqual(Guid.Empty).WithMessage("Block ID cannot be empty");

            RuleFor(x => x.BlockInstructionsId)
                .NotNull().WithMessage("Block instructions ID is required")
                .NotEqual(Guid.Empty).WithMessage("Block instructions ID cannot be empty");

            RuleFor(x => x.LeftResponseId)
                .NotNull().WithMessage("Left response ID is required")
                .NotEmpty().WithMessage("Left response ID cannot be empty")
                .NotEqual(Guid.Empty).WithMessage("Left response ID cannot be empty");

            RuleFor(x => x.RightResponseId)
                .NotNull().WithMessage("Right response ID is required")
                .NotEmpty().WithMessage("Right response ID cannot be empty")
                .NotEqual(Guid.Empty).WithMessage("Right response ID cannot be empty"); 
        }
    }
}
