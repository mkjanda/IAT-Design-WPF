using IAT.Core.Enumerations;
using IAT.Core.Models;

namespace IAT.Core.Domain;

/// <summary>
/// Instruction screen that displays text-based instructions to the participant.
/// </summary>
public class TextInstructionScreen : InstructionScreen
{
    /// <summary>
    /// Layout slot for text-only instructions.
    /// </summary>
    public override LayoutItem LayoutItem { get; init; } = LayoutItem.TextInstructions;

    /// <summary>
    /// Validates that instruction text is present (in addition to base rules).
    /// </summary>
    public override ValidationResult Validate()
    {
        var result = base.Validate();
        if (Text == string.Empty)
            result.AddError("Instructions cannot be empty.");
        return result;
    }
}
