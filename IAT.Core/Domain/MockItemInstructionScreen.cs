using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;
using IAT.Core.Models;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents an instruction screen that displays a stimulus and associated response keys, providing configurable
    /// instructional text and visual cues for user interaction in a test or application.
    /// </summary>
    /// <remarks>This class allows for dynamic association of a stimulus and response keys with the
    /// instruction screen, enabling tailored instructions and feedback based on the current context. It supports visual
    /// indicators such as error marks and outlines for correct responses to enhance user guidance and performance.
    /// Implements formatted text capabilities via the IFormattedText interface.</remarks>
    public class MockItemInstructionScreen : InstructionScreen 
    {
        /// <summary>
        /// The guid of the stimulus to be displayed on this instruction screen. This property is used to 
        /// associate a specific stimulus with the instruction screen, allowing for dynamic content based 
        /// on the stimulus being presented. The value should correspond to a valid stimulus identifier within 
        /// the context of the test or application.
        /// </summary>
        public Guid StimulusId { get; set; } = Guid.Empty;

        /// <summary>
        /// The guid of the left response key associated with the stimulus on this instruction screen.
        /// </summary>
        public Guid LeftResponseId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the right response key.
        /// </summary>
        public Guid RightResponseId { get; set; } = Guid.Empty;

        /// <summary>
        /// A value that indicates whether an error mark should be displayed on the instruction screen. This property is used to 
        /// visually signal to the user that an error has occurred, such as an incorrect response or a failure to meet certain 
        /// criteria. When set to true, an error mark (e.g., a red "X" or similar symbol) will be shown on the instruction screen, 
        /// providing immediate feedback to the user about their performance or actions. This can help users quickly identify 
        /// mistakes and adjust their behavior accordingly during the test or interaction.
        /// </summary>
        public bool ShowErrorMark { get; set; } = false;

        /// <summary>
        /// A value indicating whether  the response key the stimulus is keyed towards should be outlined on the instruction screen. 
        /// This property is used to visually highlight the response key that is associated with the stimulus being presented, 
        /// helping users quickly identify the correct response key during the test or interaction. When set to true, the response 
        /// key that corresponds to the stimulus will be outlined (e.g., with a bright border or contrasting color), providing a 
        /// clear visual cue to guide user responses and improve their performance on the task at hand.
        /// </summary>
        public bool OutlineCorrectResponse { get; set; } = false;

        /// <summary>
        /// The direction associated with the response key for the stimulus being presented on this instruction screen. 
        /// </summary>
        public KeyedDirection KeyedDirection { get; set; } = KeyedDirection.None;


        /// <summary>
        /// Gets or sets the layout item associated with this instance.
        /// </summary>
        public override LayoutItem LayoutItem { get; init; } = LayoutItem.MockItemInstructions;

        /// <summary>
        /// Validates the stimulus to ensure that it meets the necessary criteria for use in the instruction screen. This method checks that the StimulusId and ResponseKeyId are set to valid GUIDs, and that 
        /// the Instructions property is not empty. If any of these conditions are not met, the validation will fail and appropriate error messages will be added to the ValidationResult.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            var result = base.Validate();
            if (StimulusId == Guid.Empty)
                result.AddError("StimulusId must be set to a valid Guid.");
            if (LeftResponseId == Guid.Empty)
                result.AddError("LeftResponseId must be set to a valid Guid.");
            if (RightResponseId == Guid.Empty)
                result.AddError("RightResponseId must be set to a valid Guid.");
            if (Text == string.Empty)
                result.AddError("Text cannot be empty.");
            return result;
        }
    }
}
