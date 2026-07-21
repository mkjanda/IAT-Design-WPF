using IAT.Core.Enumerations;
using System.Text.Json.Serialization;

namespace IAT.Core.Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "StimulusType")]
[JsonDerivedType(typeof(ImageStimulus), "Image")]
[JsonDerivedType(typeof(TextStimulus), "Text")]
public abstract partial class Stimulus
{
    [JsonIgnore]
    public IatTest IatTest { get; set; } = null!;

    public Guid Id { get; set; } = Guid.NewGuid();

    public int OriginatingBlock { get; set; }

    public KeyedDirection KeyedDirection { get; set; } = KeyedDirection.None;

    public abstract ValidationResult Validate();
    public abstract string GetDisplayPreview();
}