using com.sun.org.apache.xml.@internal.resolver.helpers;
using IAT.Core.Enumerations;
using IAT.Core.Models; 

namespace IAT.Core.Domain;

/// <summary>
/// Represents a stimulus that displays customizable text content with configurable styling options.
/// </summary>
/// <remarks>Use the TextStimulus class to present textual information as part of an experiment or test
/// scenario. The text content and its appearance can be tailored using the Text and Style properties, respectively.
/// This class is suitable for scenarios where the precise display and formatting of text stimuli are important,
/// such as cognitive or psychological testing environments.</remarks>
public sealed class TextStimulus : Stimulus, IFormattedText 
{
    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The styling information for the text stimulus, including font, size, color, and other formatting options. 
    /// This property allows for customization of the appearance of the text when it is rendered in the user interface. 
    /// The TextStyle class encapsulates various styling attributes that can be applied to the text content, enabling 
    /// flexible presentation based on the requirements of the experiment or test scenario. By default, this property is 
    /// initialized with a new instance of TextStyle, which can be modified as needed to achieve the desired visual effect.
    /// </summary>
    public TextStyle Style { get; set; } = new TextStyle();  // Default style, can be overridden as needed

    /// <summary>
    /// Gets the layout item that determines where the content is displayed within the user interface.
    /// </summary>
    public LayoutItem LayoutItem { get; init; } = LayoutItem.Stimulus;  // This stimulus is always displayed in the stimulus area

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

