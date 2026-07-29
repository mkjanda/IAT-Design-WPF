using IAT.Core.Enumerations;
using System.Text.Json.Serialization;

namespace IAT.Core.Domain;

/// <summary>
/// Represents a stimulus in an Implicit Association Test (IAT). Stimuli can be of different types, such as images or text, 
/// and are used to elicit responses from participants during the test.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "StimulusType")]
[JsonDerivedType(typeof(ImageStimulus), "Image")]
[JsonDerivedType(typeof(TextStimulus), "Text")]
public abstract partial class Stimulus
{
    /// <summary>
    /// Gets or sets the IAT test to which this stimulus belongs.
    /// </summary>
    [JsonIgnore]
    public IatTest IatTest { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier for the stimulus.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the block from which the stimulus originated.
    /// </summary>
    public int OriginatingBlock { get; set; }

    /// <summary>
    /// Gets or sets the direction keyed for the stimulus.
    /// </summary>
    public KeyedDirection KeyedDirection { get; set; } = KeyedDirection.None;

    /// <summary>
    /// Validates the stimulus.
    /// </summary>
    /// <returns>A ValidationResult indicating whether the stimulus is valid.</returns>
    public abstract ValidationResult Validate();

    /// <summary>
    /// Gets a preview string for display purposes.
    /// </summary>
    /// <returns>A string representing the stimulus for display.</returns>
    public abstract string GetDisplayPreview();

    /// <summary>
    /// Gets or sets the display text for the stimulus (for text stimuli this is the content; for images it is typically the file name).
    /// </summary>
    public abstract string Text { get; set; }
}
