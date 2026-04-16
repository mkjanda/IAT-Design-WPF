using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Domain
{
    internal sealed partial class TextInstructionsScreen : InstructionsScreen
    {
        [ObservableProperty]
        private string _instructions = string.Empty;

        [ObservableProperty]
        private string _instructionsFontFamily = "Arial";     // default that always exists

        [ObservableProperty]
        private double _instructionsSize = 48.0;          // typical IAT size

        [ObservableProperty]
        private string _instructionsColorHex = "#FFFFFF";     // or use Color struct if you prefer

        public TextInstructionsScreen() { }

        public override ValidationResult Validate()
        {
            var result = base.Validate();
            if (Instructions == string.Empty)
                result.Fail("Instructions cannot be empty.");
            return result;
        }
    }
}
