using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;
using javax.swing.filechooser;
using System;


namespace IAT.Core.Domain
{
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
        private Guid _imageId;           // service assigns this
        private string _fileName = string.Empty;   // UI binds to this

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
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set;  } = string.Empty;

        /// <summary>
        /// Returns a string suitable for displaying as a preview of the current item.
        /// </summary>
        /// <returns>A string containing the file name to be used as a display preview.</returns>
        public override string GetDisplayPreview() => _fileName;

        /// <summary>
        /// Determines whether the current instance contains valid data for use in a test scenario.
        /// </summary>
        /// <returns>A ValidationResult indicating whether the instance is valid.</returns>
        public override ValidationResult Validate() => _imageId != Guid.Empty ? ValidationResult.Success : ValidationResult.Fail("Image ID cannot be empty.");   
    }
}