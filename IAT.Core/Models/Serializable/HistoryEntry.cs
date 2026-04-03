using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Models.Enumerations;

namespace IAT.Core.Models.Serializable
{
    /// <summary>
    /// Represents a single entry in the application's history, containing information about the time of the record,
    /// product key, error counts, and save file version.
    /// </summary>
    /// <remarks>Use this class to store and retrieve metadata related to a specific operation or session,
    /// such as when it occurred, the associated product key, and error statistics. This type is typically used for
    /// serialization or logging purposes.</remarks>
    public class HistoryEntry : IPackagePart
    {
        /// <summary>
        /// Gets or sets the URI associated with this instance.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets the MIME type associated with the history entry in XML format.
        /// </summary>
        public String MimeType => "text/xml+" + typeof(HistoryEntry).ToString();

        /// <summary>
        /// Gets the type of the package part represented by this instance.
        /// </summary>
        public PartType PackagePartType => PartType.HistoryEntry;

        /// <summary>
        /// Gets or sets the date and time when the record was opened.
        /// </summary>
        [XmlElement(ElementName = "TimeOpened", Form = XmlSchemaForm.Unqualified)]
        public String TimeOpened { get; set; } = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

        /// <summary>
        /// Gets or sets the product key associated with the current instance. This property is used to store the product key 
        /// information for the application, which may be relevant for licensing or activation purposes. 
        /// </summary>
        [XmlElement(ElementName = "ProductKey", Form = XmlSchemaForm.Unqualified)]
        public required String ProductKey { get; set; };

        /// <summary>
        /// Gets or sets the number of errors encountered during the operation.
        /// </summary>
        [XmlElement(ElementName = "ErrorCount", Form = XmlSchemaForm.Unqualified)]
        public required int ErrorCount { get; set; };

        /// <summary>
        /// Gets or sets the number of errors that have been reported.
        /// </summary>
        [XmlElement(ElementName = "ErrorsReported", Form = XmlSchemaForm.Unqualified)]
        public required int ErrorsReported { get; set; };

        /// <summary>
        /// Gets or sets the version identifier for the save file format.
        /// </summary>
        [XmlElement(ElementName = "SaveFileVersion", Form = XmlSchemaForm.Unqualified)]
        public required String Version { get; set; };


        public HistoryEntry()
        {
        }


        public void AddToXml(XElement parent)
        {
            parent.Add(new XElement("HistoryEntry", new XElement("Timestamp", TimeOpened), new XElement("Version", Version), new XElement("ErrorCount", ErrorCount),
                new XElement("ErrorsReported", ErrorsReported.ToString()), new XElement("ProductKey", ProductKey)));
        }
    }
}
