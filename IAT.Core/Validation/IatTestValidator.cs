// IAT.Core/Validation/IatTestValidator.cs
using FluentValidation;
using IAT.Core.Domain;

/// <summary>
/// Validator for IAT test structure, ensuring all components meet required criteria.
/// </summary>
public class IatTestValidator : AbstractValidator<IatTest>
{
    /// <summary>
    /// Initializes a new instance of the IatTestValidator class with validation rules for IAT test structure.
    /// </summary>
    /// <param name="stimulusValidator">Validator for individual stimuli.</param>
    /// <param name="trialValidator">Validator for individual trials.</param>
    /// <param name="blockValidator">Validator for individual blocks.</param>
    /// <param name="instructionValidator">Validator for individual instruction screens.</param>
    public IatTestValidator(
        IValidator<Stimulus> stimulusValidator,
        IValidator<Trial> trialValidator,
        IValidator<Block> blockValidator,
        IValidator<InstructionScreen> instructionValidator)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("IAT name is required")
            .MaximumLength(200);

        RuleFor(x => x.AllBlocks)
            .Must(blocks => blocks.Count == 7)
            .WithMessage("Exactly 7 blocks are required for a standard IAT");

        RuleForEach(x => x.AllTrials)
            .SetValidator(trialValidator);

        RuleForEach(x => x.AllStimuli)
            .SetValidator(stimulusValidator);

        RuleForEach(x => x.AllInstructionScreens)
            .SetValidator(instructionValidator);

        RuleForEach(x => x.AllBlocks)
            .SetValidator(blockValidator);

        // Cross-cutting rule: every stimulus must be used
        RuleFor(x => x)
            .Must(test => test.AllStimuli.All(s => test.AllTrials.Any(t => t.StimulusId == s.Id)))
            .WithMessage("Every stimulus must be used in at least one trial");
    }
}