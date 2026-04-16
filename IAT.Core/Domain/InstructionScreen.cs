using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Validation;
using System.IO;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IAT.Core.Domain
{
    public partial class InstructionsScreen : ObservableObject
    {
        /// <summary>
        /// Gets or sets the unique identifier for the instructions resource associated with this instance.
        /// </summary>
        [ObservableProperty]
        private Guid _id = Guid.Empty;

        /// <summary>
        /// Gets a value indicating whether this item is a header item.
        /// </summary>
        public bool IsHeaderItem => false;

        /// <summary>
        /// Gets a value indicating whether the current item can be expanded to show additional content or children.
        /// </summary>
        public bool IsExpandable => false;

        /// <summary>
        /// Gets or sets the key used to continue an operation or process.
        /// </summary>
        [ObservableProperty]
        protected string _continueKey = " ";

        [ObservableProperty]
        private string _continueInstructions = string.Empty;

        [ObservableProperty]
        private string _continueInstructionsFontFamily = "Arial";     // default that always exists

        [ObservableProperty]
        private double _continueInstructionsSize = 48.0;          // typical IAT size

        [ObservableProperty]
        private string _continueInstructionsColorHex = "#FFFFFF";     // or use Color struct if you prefer


        /// <summary>
        /// Gets or sets the unique identifier for the continue instructions resource.
        /// </summary>
        [ObservableProperty]
        private Guid _continueInstructionsId = Guid.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the preview instance.
        /// </summary>
        [ObservableProperty]
        protected Guid _previewId = Guid.Empty;

        /// <summary>
        /// The InstructionScreen object constructor
        /// </summary>
        public InstructionsScreen() { }

        /// <summary>
        /// Validates the current instruction screen definition and returns the result of the validation.
        /// </summary>
        /// <returns>A ValidationResult indicating whether the instruction screen definition is valid. Returns
        /// ValidationResult.Success if validation passes.</returns>
        /// <exception cref="Exception">Thrown if the instruction screen type is blank or if the continue instructions text is empty.</exception>
        public virtual ValidationResult Validate()
        {
            var validationResult = new ValidationResult();
            if (ContinueInstructions == string.Empty)
                validationResult.Fail("Continue instructions text cannot be empty");
            return validationResult;
        }
    }
}
