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
    /// Gets or sets the file name (or full path) of the image. Display text is derived from this.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the text to be displayed for the image stimulus, which is derived from the file name. 
    /// This property extracts the file name from the full path and provides it as a displayable string.
    /// Setting is not supported; set <see cref="FileName"/> instead.
    /// </summary>
    public override string Text
    {
        get => string.IsNullOrEmpty(FileName) ? string.Empty : Path.GetFileName(FileName);
        set => throw new NotImplementedException(
            "The Text property is derived from the FileName and cannot be set directly. Please set the FileName property instead.");
    }

    /// <summary>
    /// Returns a string suitable for displaying as a preview of the current item.
    /// </summary>
    /// <returns>A string containing the file name to be used as a display preview.</returns>
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
