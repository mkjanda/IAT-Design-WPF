using IAT.Core.Enumerations;
using System.Text.Json.Serialization;

namespace IAT.Core.Domain;

/// <summary>
/// Defines a base class for stimuli used in an IAT experiment, providing common 
/// properties and abstract methods for validation and display preview.
/// </summary>
public abstract partial class Stimulus
{
    /// <summary>
    /// Gets or sets the IAT test.
    /// </summary>
    [JsonIgnore]
    public IatTest IatTest { get; set; } = null!;

    /// <summary>
    /// The guid the stimulus will be identifiedby
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();



    /// <summary>
    /// Gets or sets the originating block for the stimulus.
    /// </summary>
    public int OriginatingBlock { get; set; }

    /// <summary>
    /// Gets or sets the keyed direction for the stimulus.
    /// </summary>
    public KeyedDirection KeyedDirection { get; set; } = KeyedDirection.None;

    // Pure domain behavior only
    public abstract ValidationResult Validate();
    public abstract string GetDisplayPreview();

    /// <summary>
    /// Returns the display preview so ComboBoxes and lists bound with <c>{Binding}</c>
    /// show a human-readable label instead of the type name.
    /// </summary>
    public override string ToString() => GetDisplayPreview();
}