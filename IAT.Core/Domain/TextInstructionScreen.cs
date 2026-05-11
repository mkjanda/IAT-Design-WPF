using IAT.Core.Enumerations;
using IAT.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents a screen that displays text-based instructions to the user.
    /// </summary>
    /// <remarks>This class specializes the base InstructionsScreen to present instructions as text. It
    /// provides additional validation to ensure that the instructions are not empty, in addition to any validation
    /// performed by the base class.</remarks>
    public class TextInstructionScreen : InstructionScreen
    {
        /// <summary>
        /// Gets the layout item that defines how the content is presented.
        /// </summary>
        public override LayoutItem LayoutItem { get; init; } = LayoutItem.TextInstructions;

        /// <summary>
        /// Validates the current object and returns the result of the validation process.
        /// </summary>
        /// <remarks>This method extends the base validation by ensuring that the instructions are not
        /// empty. Additional validation rules from the base class are also applied.</remarks>
        /// <returns>A <see cref="ValidationResult"/> that contains the results of the validation. The result includes any errors
        /// found, such as when the instructions are empty.</returns>
        public override ValidationResult Validate()
        {
            var result = base.Validate();
            if (Text == string.Empty)
                result.AddError("Instructions cannot be empty.");
            return result;
        }
    }
}
