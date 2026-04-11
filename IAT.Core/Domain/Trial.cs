using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents a single trial within an experiment, containing information about the associated stimulus, trial
    /// number, block assignment, and keyed direction.
    /// </summary>
    /// <remarks>The Trial class is typically used to encapsulate all relevant data for a single experimental
    /// trial, including references to the stimulus presented and the participant's response direction. It supports
    /// property change notification for data binding scenarios. Validation logic is provided to ensure that each trial
    /// is correctly configured before use.</remarks>
    public partial class Trial : ObservableObject
    {
        /// <summary>
        /// Gets or sets the unique identifier for this instance.
        /// </summary>
        [ObservableProperty]
        private Guid _id = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the unique identifier of the associated stimulus.
        /// </summary>
        [ObservableProperty]
        private Guid _stimulusId;

        /// <summary>
        /// The guid of the preview image of the trial.
        /// </summary>
        [ObservableProperty]
        private Guid _previewId;

        /// <summary>
        /// Gets or sets the current trial number.
        /// </summary>
        [ObservableProperty]
        private int _trialNumber;

        /// <summary>
        /// Gets or sets the current keyed direction value.
        /// </summary>
        [ObservableProperty]
        private KeyedDirection _keyedDirection = KeyedDirection.None;

        /// <summary>
        /// Gets or sets the block number associated with this instance.
        /// </summary>
        [ObservableProperty]
        private int _blockNumber;

        /// <summary>
        /// Gets or sets the index of the originating block associated with this instance.
        /// </summary>
        [ObservableProperty]
        private int _originatingBlock;

        /// <summary>
        /// Validates the specified stimulus against the trial's configuration and requirements.
        /// </summary>
        /// <remarks>Validation checks include ensuring the trial references a valid stimulus, has a keyed
        /// direction, and that the stimulus is appropriate for the trial's keying. The method returns early with a
        /// failure result if any validation fails.</remarks>
        /// <param name="stimulus">The stimulus to validate. Must not be null and should match the expected type and configuration for the
        /// trial.</param>
        /// <returns>A ValidationResult indicating whether the stimulus is valid for this trial. Returns ValidationResult.Success
        /// if valid; otherwise, a failed ValidationResult with an appropriate error message.</returns>
        public ValidationResult Validate()
        {
            if (StimulusId == Guid.Empty)
                return ValidationResult.Fail("Trial must reference a valid stimulus");

            if (KeyedDirection == KeyedDirection.None)
                return ValidationResult.Fail("Every trial must have a keyed direction");

            return ValidationResult.Success;
        }
    }
}
