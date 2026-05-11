
using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;
using IAT.Core.Models;


namespace IAT.Core.Domain
{
    /// <summary>
    /// Defined an instruction screen with a response key in the uppper corners and a blockof text taking upthe remainder of the
    /// test box
    /// </summary>
    public class KeyedInstructionScreen : InstructionScreen, IFormattedText
    {
        /// <summary>
        /// Gets or sets the unique identifier for the response key.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the unique identifier for the left response.
        /// </summary>
        public Guid LeftResponseId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the right response.
        /// </summary>
        public Guid RightResponseId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets the layout item associated with keyed instructions.
        /// </summary>
        public override LayoutItem LayoutItem { get; init; } = LayoutItem.KeyedInstructions;

        /// <summary>
        ///  Validates the current object and returns the result of the validation.
        /// </summary>
        /// <returns>A <see cref="ValidationResult"/> containing any validation errors found. The result will include errors if
        /// <c>ResponseKeyId</c> is not set to a valid <see cref="Guid"/> or if <c>Instructions</c> is empty.</returns>
        public override ValidationResult Validate()
        {
            var result = base.Validate();
            if (ResponseKeyId == Guid.Empty)
                result.AddError("ResponseKeyId must be set to a valid Guid.");
            if (Instructions == string.Empty)
                result.AddError("Instructions cannot be empty.");
            return result;
        }
    }
}
