using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;
using IAT.Core.Models;

namespace IAT.Core.Domain;

/// <summary>
/// Instruction screen with response keys in the upper corners and a block of text
/// occupying the remainder of the test area.
/// </summary>
public class KeyedInstructionScreen : InstructionScreen, IFormattedText
{
    /// <summary>
    /// Unique identifier for the left response key.
    /// </summary>
    public Guid LeftResponseId { get; set; } = Guid.Empty;

    /// <summary>
    /// Unique identifier for the right response key.
    /// </summary>
    public Guid RightResponseId { get; set; } = Guid.Empty;

    /// <summary>
    /// Layout slot for keyed instructions.
    /// </summary>
    public override LayoutItem LayoutItem { get; init; } = LayoutItem.KeyedInstructions;

    /// <summary>
    /// Validates that both response keys are set and instruction text is present.
    /// </summary>
    public override ValidationResult Validate()
    {
        var result = base.Validate();
        if (LeftResponseId == Guid.Empty)
            result.AddError("LeftResponseId must be set to a valid Guid.");
        if (RightResponseId == Guid.Empty)
            result.AddError("RightResponseId must be set to a valid Guid.");
        if (Text == string.Empty)
            result.AddError("Instructions cannot be empty.");
        return result;
    }
}
