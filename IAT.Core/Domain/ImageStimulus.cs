using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Enumerations;


namespace IAT.Core.Domain
{
    public sealed partial class ImageStimulus : Stimulus
    {
        [ObservableProperty]
        private Guid _imageId;           // service assigns this

        [ObservableProperty]
        private string _fileName = string.Empty;   // UI binds to this


        /// <summary>
        /// Returns a string suitable for displaying as a preview of the current item.
        /// </summary>
        /// <returns>A string containing the file name to be used as a display preview.</returns>
        public override string GetDisplayPreview() => FileName;

        /// <summary>
        /// Determines whether the current instance contains valid data for use in a test scenario.
        /// </summary>
        /// <returns>A ValidationResult indicating whether the instance is valid.</returns>
        public override ValidationResult Validate() => ImageId != Guid.Empty ? ValidationResult.Success : ValidationResult.Fail("Image ID cannot be empty.");   
    }
}