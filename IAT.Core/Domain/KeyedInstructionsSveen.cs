
using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;
namespace IAT.Core.Domain
{
    /// <summary>
    /// Defined an instruction screen with a response key in the uppper corners and a blockof text taking upthe remainder of the
    /// test box
    /// </summary>
    internal sealed partial class KeyedInstructionsScreen : InstructionsScreen
    {
        /// <summary>
        /// Gets or sets the unique identifier for the response key.
        /// </summary>
        [ObservableProperty]
        private Guid _responseKeyId;

        /// <summary>
        /// Gets or sets the instructions associated with the current context.
        /// </summary>
        [ObservableProperty]
        private string _instructions = string.Empty;

        /// <summary>
        /// Gets or sets the font family used to display instructions.
        /// </summary>
        [ObservableProperty]
        private string _instructionsFontFamily = "Arial";     // default that always exists

        /// <summary>
        /// Gets or sets the size, in bytes, of the instructions area.
        /// </summary>
        /// <remarks>This property typically represents the size of the Import Address Table (IAT) or a
        /// similar instructions region. Adjust this value if the instructions area size differs from the
        /// default.</remarks>
        [ObservableProperty]
        private double _instructionsSize = 48.0;          // typical IAT size   

        /// <summary>
        /// Gets or sets the color of the instructions as a hexadecimal string.
        /// </summary>
        [ObservableProperty]
        private string _instructionsColorHex = "#FFFFFF";     // or use Color struct if you prefer

        /// <summary>
        /// Initializes a new instance of the KeyedInstructionsScreen class.
        /// </summary>
        public KeyedInstructionsScreen() { }

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
