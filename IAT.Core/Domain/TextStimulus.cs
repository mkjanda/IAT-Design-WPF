using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;

namespace IAT.Core.Domain
{
    public sealed partial class TextStimulus : Stimulus
    {
        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private string _fontFamily = "Arial";     // default that always exists

        [ObservableProperty]
        private double _fontSize = 48.0;          // typical IAT size

        [ObservableProperty]
        private string _colorHex = "#FFFFFF";     // or use Color struct if you prefer

        /// <summary>
        /// Pure domain preview. This is the only thing GetDisplayPreview should return for a TextStimulus, and it should be the Text property. 
        /// This is what will be shown in the UI when listing stimuli, and it should be a simple, human-readable string that represents the content of the stimulus. 
        /// It should not include font, color, or other styling information, as those are not relevant for a quick preview. The Text property itself is the 
        /// most direct representation of the stimulus content and is what users will expect to see when they look at a list of stimuli.
        /// </summary>
        /// <returns></returns>
        public override string GetDisplayPreview()
        {
            // Simple, fast string for UI lists and quick reference
            return string.IsNullOrWhiteSpace(Text)
                ? "(empty text stimulus)"
                : Text;   // ← THIS is the correct value to return
        }

        /// <summary>
        /// Determines whether the current instance contains valid text for use in a test scenario.
        /// </summary>
        /// <returns>true if the Text property is not null, empty, or consists only of white-space characters; otherwise, false.</returns>
        public override ValidationResult Validate() 
            => !string.IsNullOrWhiteSpace(Text) ? ValidationResult.Success : ValidationResult.Fail("Text cannot be empty or whitespace.");
    }
}

