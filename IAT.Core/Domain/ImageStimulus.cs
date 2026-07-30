using IAT.Core.Enumerations;
using System.IO;

namespace IAT.Core.Domain;

/// <summary>
/// Encapsulates the properties and behaviors of an image stimulus used in an Implicit Association Test (IAT). 
/// This class inherits from the base Stimulus class and includes specific properties related to image stimuli, 
/// such as a unique identifier for the image, the file name for display purposes, and a URI for locating the 
/// image resource within the experiment package. The ImageStimulus class provides methods for validating its data 
/// and generating a display preview based on the file name. It is designed to be used within the context of an IAT 
/// test, allowing for the inclusion of visual stimuli as part of the test design.
/// </summary>
public sealed partial class ImageStimulus : Stimulus
{
    /// <summary>
    /// The URI of the image file within the package. This property is used to locate and load the image resource when needed. 
    /// The URI should be relative to the package structure and point to the location of the image file included in the experiment package.
    /// </summary>
    public Uri? PackageUri { get; set; }

    /// <summary>
    /// Gets or sets the alternative text for the image stimulus. This text is used for accessibility purposes and 
    /// provides a description of the image content.
    /// </summary>
    public string AltText { get; set; } = string.Empty;

    /// <summary>
    /// Leaf file name of the image (e.g. "face.png"). Prefer storing only the name, not a full
    /// disk path — the package owns image bytes by <see cref="Stimulus.Id"/>. Full paths that
    /// slip in are still stripped by <see cref="Text"/> / <see cref="GetDisplayPreview"/>.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Display text for lists, trial/instruction previews, and search. Always the leaf file name
    /// via <see cref="Path.GetFileName"/> so a legacy full path in <see cref="FileName"/> never
    /// leaks into the UI. Setting is not supported; set <see cref="FileName"/> instead.
    /// </summary>
    public override string Text
    {
        get => string.IsNullOrEmpty(FileName) ? string.Empty : Path.GetFileName(FileName);
        set;
    }

    /// <summary>
    /// Returns the leaf file name for UI previews (same as <see cref="Text"/>).
    /// </summary>
    public override string GetDisplayPreview() => Text;

    /// <summary>
    /// Validates the properties of the ImageStimulus instance to ensure that they meet the required criteria.
    /// </summary>
    /// <returns>A ValidationResult indicating whether the instance is valid.</returns>
    public override ValidationResult Validate() =>
        Id != Guid.Empty
            ? ValidationResult.Success
            : ValidationResult.Fail("Image stimulus ID cannot be empty.");
}
