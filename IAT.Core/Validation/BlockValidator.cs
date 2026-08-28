using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Domain;

namespace IAT.Core.Validation
{
    public class BlockValidator : AbstractValidator<Block>
    {
        /// <summary>
        /// Placeholder shown in the Blocks-tab layout preview when instructions are empty.
        /// Export must never ship this string; validation rejects it as "unset".
        /// </summary>
        public const string PlaceholderInstructions = "Block Instructions";

        public BlockValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Block ID is required")
                .NotEqual(Guid.Empty).WithMessage("Block ID cannot be empty");

            RuleFor(x => x.BlockInstructionsId)
                .NotEqual(Guid.Empty)
                .WithMessage("Block instructions must be set (FormattedText Id is required)");

            RuleFor(x => x.BlockInstructions)
                .Must(BeRealInstructions)
                .WithMessage(
                    "Block instructions text is required. Clear the placeholder and enter the text participants will see during the block.");

            RuleFor(x => x.LeftResponseId)
                .NotEqual(Guid.Empty)
                .WithMessage("Left response ID cannot be empty");

            RuleFor(x => x.RightResponseId)
                .NotEqual(Guid.Empty)
                .WithMessage("Right response ID cannot be empty");
        }

        /// <summary>
        /// True when the text is present and is not the UI placeholder.
        /// </summary>
        public static bool BeRealInstructions(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            return !string.Equals(text.Trim(), PlaceholderInstructions, StringComparison.OrdinalIgnoreCase);
        }
    }
}
