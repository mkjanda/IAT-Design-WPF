using System.IO.Packaging;
using System.Xml.Serialization;
using IAT.Core.Models.Enumerations;
using java.net;

namespace IAT.Core.Models
{
    /// <summary>
    /// Pure data model for an IAT block.
    /// Used for XML serialization, XSLT reports, and server upload.
    /// No UI logic, no side effects, no global state.
    /// </summary>
    [XmlRoot("Block")]
    public class IATBlock
    {
        /// <summary>
        /// Gets or sets the unique identifier associated with this instance.
        /// </summary>
        [XmlAttribute("Id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of block represented by this instance.
        /// </summary>
        [XmlElement("Type")]
        public PartType Type { get; set; } = PartType.Block;

        /// <summary>
        /// Gets or sets the name associated with the current instance.
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URI that provides additional instructions or guidance.
        /// </summary>
        [XmlElement("InstructionsUri")]
        public string InstructionsUri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URI of the preview resource associated with this item.
        /// </summary>
        [XmlElement("PreviewUri")]
        public string PreviewUri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the object uses dynamic keys for identification or access.
        /// </summary>
        [XmlElement("IsDynamicallyKeyed")]
        public bool IsDynamicallyKeyed { get; set; }

        /// <summary>
        /// Gets the collection of URIs that identify the stimuli associated with this instance.
        /// </summary>
        /// <remarks>The collection is initialized to an empty list and is immutable after object
        /// construction. Each URI should uniquely identify a stimulus resource relevant to the context in which this
        /// property is used.</remarks>
        [XmlArray("StimulusUris")]
        [XmlArrayItem("StimulusUri")]
        public List<Uri> StimulusUris { get; init; } = new();
    }
}