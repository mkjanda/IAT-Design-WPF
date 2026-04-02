using FluentValidation;
using IAT.Core.Models;
using IAT.Core.Models.Enumerations;
using net.sf.saxon.om;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace IAT.Core.Services.Validation
{
    /// <summary>
    /// Provides static methods and properties for validating items and tracking validation errors within the
    /// application.
    /// </summary>
    /// <remarks>This class maintains a shared dictionary of validation errors for items implementing the
    /// IValidatedItem interface. It is intended for use as a centralized validation utility and is not thread-safe. All
    /// members are static and should be accessed directly without instantiation.</remarks>
    public class TrialValidator : AbstractValidator<Trial>
    {
        /// <summary>
        /// Initializes a new instance of the TrialValidator class with validation rules for trial data.
        /// </summary>
        /// <remarks>The validator enforces that each trial has a keyed direction assigned, a non-empty
        /// stimulus identifier, and a properly initialized stimulus. Validation error messages include contextual
        /// information such as the trial and block numbers to aid in identifying issues.</remarks>
        public TrialValidator()
        {
            RuleFor(x => x.KeyedDirection).NotEqual(KeyedDirection.None).
                WithMessage(trial => $"Item #{trial.TrialNumber} in IAT block #{trial.BlockNumber} has not been assigned a keyed direction.").WithState(x => x.Id);

            RuleFor(x => x.StimulusId).NotEqual(Guid.Empty).
                WithMessage(trial => $"Item #{trial.TrialNumber} in IAT block #{trial.BlockNumber} has not been assigned a stimulus.").WithState(x => x.Id);

            RuleFor(x => x.Stimulus.IsInitialized).NotEqual(false).WithMessage(trial => $"The image stimulus for item #{trial.TrialNumber} in IAT block #{trial.BlockNumber} has not been properly defined.").
                WithState(x => x.Id);
        }
    }
}
