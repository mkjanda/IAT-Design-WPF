using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Services;

namespace IAT.Core.Models
{
    public class HistoryEntry
    {
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
        public String ProductKey { get; set; } = "N/A";

        /// <summary>
        /// Gets or sets the number of errors encountered during the operation.
        /// </summary>
        [XmlElement(ElementName = "ErrorCount", Form = XmlSchemaForm.Unqualified)]
        public int ErrorCount { get; set; } = -1;

        /// <summary>
        /// Gets or sets the number of errors that have been reported.
        /// </summary>
        [XmlElement(ElementName = "ErrorsReported", Form = XmlSchemaForm.Unqualified)]
        public int ErrorsReported { get; set; } = -1;

        /// <summary>
        /// Gets or sets the version identifier for the save file format.
        /// </summary>
        [XmlElement(ElementName = "SaveFileVersion", Form = XmlSchemaForm.Unqualified)]
        public String Version { get; set; } = null;

        [XmlIgnore]
        private readonly ILocalStorageService _localStorage;
        public HistoryEntry(ILocalStorageService localStorage)
        {
            Version = localStorage[localStorage.Field.Version];
            ProductKey = localStorage[localStorage.Field.ProductKey];
            ErrorCount = ErrorReporter.Errors;
            ErrorsReported = ErrorReporter.ErrorsReported;
            _localStorage = localStorage;
        }

        public HistoryEntry(XElement root, ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            if (root.Element("Timestamp") != null)
                TimeOpened = root.Element("Timestamp").Value;
            if (root.Element("Version") != null)
                Version = root.Element("Version").Value;
            if (root.Element("ErrorCount") != null)
                ErrorCount = Convert.ToInt32(root.Element("ErrorCount").Value);
            if (root.Element("ErrorsReported") != null)
                ErrorsReported = Convert.ToInt32(root.Element("ErrorsReported").Value);
            if (root.Element("ProductKey") != null)
                ProductKey = root.Element("ProductKey").Value;
        }

        public void AddToXml(XElement parent)
        {
            parent.Add(new XElement("HistoryEntry", new XElement("Timestamp", TimeOpened), new XElement("Version", Version), new XElement("ErrorCount", ErrorCount),
                new XElement("ErrorsReported", ErrorsReported.ToString()), new XElement("ProductKey", ProductKey)));
        }
    }
}
