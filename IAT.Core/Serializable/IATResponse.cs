using IAT.Core.Enumerations;
using java.io;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// The TrialResponse class represents the response data for a single trial in an Implicit Association Test (IAT). It includes properties for 
    /// block number, item number, response time, error status, and presentation number. This class is designed to be serialized to and deserialized 
    /// from XML format, with appropriate XML element attributes to define the structure of the XML representation. Each property corresponds to a 
    /// specific aspect of the trial response, allowing for detailed tracking and analysis of participant responses during the IAT.
    /// </summary>
    public class TrialResponse
    {
        /// <summary>
        /// Gets or sets the block number associated with the current entity.
        /// </summary>
        [XmlElement("BlockNum", Form = XmlSchemaForm.Unqualified, DataType = "int")]
        public int BlockNumber { get; set; } = 0;

        /// <summary>
        /// Gets or sets the unique number that identifies the item.
        /// </summary>
        [XmlElement("ItemNum", Form = XmlSchemaForm.Unqualified, DataType = "int")]
        public int ItemNumber { get; set; } = 0;

        /// <summary>
        /// Gets or sets the response time for the operation, measured in milliseconds.
        /// </summary>
        [XmlElement("ResponseTime", Form = XmlSchemaForm.Unqualified, DataType = "long")]
        public long ResponseTime { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether an error has occurred.
        /// </summary>
        [XmlElement("Error", Form = XmlSchemaForm.Unqualified, DataType = "boolean")]
        public bool Error { get; set; } = false;

        /// <summary>
        /// Gets or sets the presentation number associated with the current instance.
        /// </summary>
        [XmlElement("PresentationNum", Form = XmlSchemaForm.Unqualified, DataType = "int")]
        public int PresentationNumber { get; set; } = 0;
    }

    /// <summary>
    /// Represents the result set of an Implicit Association Test (IAT), containing the collection of individual trial
    /// responses.
    /// </summary>
    /// <remarks>This class is typically used for serialization and deserialization of IAT results in XML
    /// format. The structure aligns with the expected schema for IAT result data exchange.</remarks>
    [XmlRoot("IATResultSet")]
    public class IATResponse
    {
        /// <summary>
        /// Gets or sets the collection of trial responses associated with the current session.
        /// </summary>
        [XmlArray]
        [XmlArrayItem("IATResponseSetElement")]
        public List<TrialResponse> Responses { get; set; } = new();
    }
}