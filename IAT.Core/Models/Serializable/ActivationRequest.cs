using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Models.Serializable
{
    /// <summary>
    /// Represents a request to activate a product, including user and product information required for activation.
    /// </summary>
    /// <remarks>This class is typically used to serialize or deserialize activation requests in XML format
    /// for product registration workflows. All properties are required and must be provided to create a valid
    /// activation request.</remarks>
    class ActivationRequest 
    {
        [XmlElement("ProductCode", Form = XmlSchemaForm.Unqualified)]
        public required string ProductCode { get; init; }

        [XmlElement("FName", Form = XmlSchemaForm.Unqualified)]
        public required string FirstName { get; init; }

        [XmlElement("LName", Form = XmlSchemaForm.Unqualified)]
        public required string LastName { get; init; }

        [XmlElement("EMail", Form = XmlSchemaForm.Unqualified)]
        public required string EMail { get; init; }

        [XmlElement("Title", Form = XmlSchemaForm.Unqualified)]
        public required string Title { get; init;  }

        public ActivationRequest()
        {
        }
    }
}
