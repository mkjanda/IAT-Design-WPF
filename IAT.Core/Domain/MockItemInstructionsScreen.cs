using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IAT.Core.Domain
{
    internal sealed partial class MockItemInstructionsScreen : InstructionsScreen
    {
        /// <summary>
        /// The guid of the stimulus to be displayed on this instruction screen. This property is used to 
        /// associate a specific stimulus with the instruction screen, allowing for dynamic content based 
        /// on the stimulus being presented. The value should correspond to a valid stimulus identifier within 
        /// the context of the test or application.
        /// </summary>
        [ObservableProperty]
        private Guid _stimulusId = Guid.Empty;

        /// <summary>
        /// The guid of the respponse key that accompanies the stimulus on this instruction screen. This property is used to link 
        /// a specific response key to the instruction screen, enabling the system to recognize and process user input in relation 
        /// to the displayed stimulus. The value should correspond to a valid response key identifier within the context of the 
        /// test or application.
        /// </summary>
        [ObservableProperty]
        private Guid _responseKeyId = Guid.Empty;

        /// <summary>
        /// The text of the instructions that fa;; bel;ow the stimulus and response key on this instruction screen. This property is 
        /// used to provide guidance or information to the user regarding the stimulus and response key, helping them understand 
        /// what is expected of them during the test or interaction. The instructions should be clear and concise to ensure that 
        /// users can easily comprehend the requirements of the task at hand.
        /// </summary>
        [ObservableProperty]
        private string _instructions = string.Empty;

        /// <summary>
        /// A value that indicates whether an error mark should be displayed on the instruction screen. This property is used to 
        /// visually signal to the user that an error has occurred, such as an incorrect response or a failure to meet certain 
        /// criteria. When set to true, an error mark (e.g., a red "X" or similar symbol) will be shown on the instruction screen, 
        /// providing immediate feedback to the user about their performance or actions. This can help users quickly identify 
        /// mistakes and adjust their behavior accordingly during the test or interaction.
        /// </summary>
        [ObservableProperty]
        private bool _showErrorMark = false;

        /// <summary>
        /// A value indicating whether  the response key the stimulus is keyed towards should be outlined on the instruction screen. 
        /// This property is used to visually highlight the response key that is associated with the stimulus being presented, 
        /// helping users quickly identify the correct response key during the test or interaction. When set to true, the response 
        /// key that corresponds to the stimulus will be outlined (e.g., with a bright border or contrasting color), providing a 
        /// clear visual cue to guide user responses and improve their performance on the task at hand.
        /// </summary>
        [ObservableProperty]
        private bool _outlineCorrectResponse = false;

        /// <summary>
        /// The direction associated with the response key for the stimulus being presented on this instruction screen. 
        /// </summary>
        [ObservableProperty]
        private KeyedDirection _keyedDirection = KeyedDirection.None;

        /// <summary>
        /// Gets or sets the font family used for displaying instructions.
        /// </summary>
        [ObservableProperty]
        private string _instructionsFontFamily = "Arial";     // default that always exists

        /// <summary>
        /// Gets or sets the size, in bytes, of the instructions section.
        /// </summary>
        /// <remarks>This property typically represents the Import Address Table (IAT) size. Adjust this
        /// value if the instructions section size differs from the default.</remarks>
        [ObservableProperty]
        private double _instructionsSize = 48.0;          // typical IAT size

        /// <summary>
        /// Gets or sets the color used for displaying instructions, represented as a hexadecimal string.
        /// </summary>
        /// <remarks>The color value should be specified in standard hexadecimal format (e.g., "#FFFFFF"
        /// for white). This property is typically used to customize the appearance of instructional text in the user
        /// interface.</remarks>
        [ObservableProperty]
        private string _instructionsColorHex = "#FFFFFF";     // or use Color struct if you prefer

        /// <summary>
        /// Initializes a new instance of the MockItemInstructionsScreen class.
        /// </summary>
        public MockItemInstructionsScreen() { }

        public override ValidationResult Validate()
        {
            var result = base.Validate();
            if (StimulusId == Guid.Empty)
                result.Fail("StimulusId must be set to a valid Guid.");
            if (ResponseKeyId == Guid.Empty)
                result.Fail("ResponseKeyId must be set to a valid Guid.");
            if (Instructions == string.Empty)
                result.Fail("Instructions cannot be empty.");
            return result;
        }
    }
}
