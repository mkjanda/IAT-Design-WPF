using IAT.Core.Enumerations;
using IAT.Core.Models;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Serializable;
using CommunityToolkit.Mvvm.ComponentModel;
using net.sf.saxon.style;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents a block of items within an IAT (Implicit Association Test), containing presentations, instructions,
    /// response keys, and related configuration for a test segment.
    /// </summary>
    /// <remarks>A block manages a collection of items (stimuli or presentations) and their associated
    /// metadata, such as instructions and response keys. It supports alternation with other blocks, dynamic keying, and
    /// provides methods for validation, serialization, and previewing. Blocks are typically managed as part of an IAT's
    /// structure and can be added, removed, or reordered within the test. Thread safety is not guaranteed; external
    /// synchronization is required for concurrent access.</remarks>
    public partial class Block : ObservableObject
    {

        /// <summary>
        /// Gets or sets the unique identifier for the object.
        /// </summary>
        [ObservableProperty]
        private Guid _id = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the number of trials to perform in the operation.
        /// </summary>
        [ObservableProperty]
        private int _numPresentations = 0;

        /// <summary>
        /// Gets or sets the alternation group associated with the element.
        /// </summary>
        /// <remarks>The alternation group defines a set of mutually exclusive options for the element. If set to null, no
        /// alternation group is applied.</remarks>
        [ObservableProperty]
        private Guid _alternationGroupId = Guid.Empty;

        /// <summary>
        /// Gets or sets the URI where instructions for completing the process can be accessed.
        /// </summary>
        [ObservableProperty]
        private List<Guid> _instructionsIds = new();

        /// <summary>
        /// Gets or sets the URI used to display the left-side response in a comparison or review scenario.
        /// </summary>
        [ObservableProperty]
        private Guid _leftResponseId = Guid.Empty;

        /// <summary>
        /// Gets or sets the URI used to display the right-side response in the user interface.
        /// </summary>
        [ObservableProperty]
        private Guid _rightResponseId = Guid.Empty;

        /// <summary>
        /// Gets the collection of unique identifiers for the associated trials.
        /// </summary>
        [ObservableProperty]
        private List<Guid> _trialIds = new();

        /// <summary>
        /// Indicates whether the item is a header item.
        /// </summary>
        public static bool IsHeaderItem => true;

        /// <summary>
        /// Determines if the representation of the item expands to show sub-items or details. For a block, 
        /// this is typically true, as it can contain multiple trials and instructions.
        /// </summary>
        public static bool IsExpandable => true;

        /// <summary>`
        /// Gets or sets the URI that identifies the cryptographic key.
        /// </summary>
        [ObservableProperty]
        private Guid _keyId = Guid.Empty;

        /// <summary>
        /// Gets the block number associated with this instance.
        /// </summary>
        [ObservableProperty]
        public int _blockNumber = 0;

        /// <summary>
        /// Initializes a new instance of the Block class.
        /// </summary>
        public Block() { }


        /// <summary>
        /// Gets a value indicating whether an alternate item is associated with this instance.
        /// </summary>
        public bool HasAlternateBlock
        {
            get
            {
                return AlternationGroupId != Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the name associated with the current instance.
        /// </summary>
        [ObservableProperty]
        private string _name = String.Empty;

        /// <summary>
        /// Validates the current instance of the Block class, ensuring that all required properties are set and contain valid values.
        /// </summary>
        /// <returns>A ValidationResult object containing any validation errors.</returns>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            if (NumPresentations <= 0)
                result.AddError("Number of presentations must be greater than zero.");
            if (InstructionsIds == null || InstructionsIds.Count == 0)
                result.AddError("At least one instructions ID must be set.");
            if (LeftResponseId == Guid.Empty)
                result.AddError("Left response ID must be set.");
            if (RightResponseId == Guid.Empty)
                result.AddError("Right response ID must be set.");
            return result;
        }
    }
}

