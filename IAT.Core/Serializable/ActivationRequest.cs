using System.Xml.Serialization;
using System.Xml.Schema;
using MediatR;
using IAT.Core.Enumerations;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents a request to activate a product, including user and product information required for activation.
    /// </summary>
    /// <remarks>This class is typically used to serialize or deserialize activation requests in XML format
    /// for product registration workflows. All properties are required and must be provided to create a valid
    /// activation request.</remarks>
    public class ActivationRequest 
    {
        /// <summary>
        /// Gets the unique code that identifies the product.
        /// </summary>
        [XmlElement("ProductCode", Form = XmlSchemaForm.Unqualified)]
        public required string ProductCode { get; init; } = String.Empty;

        /// <summary>
        /// Gets the first name of the person.
        /// </summary>
        [XmlElement("FName", Form = XmlSchemaForm.Unqualified)]
        public required string FirstName { get; init; } = String.Empty;

        /// <summary>
        /// Gets the last name of the person.
        /// </summary>
        [XmlElement("LName", Form = XmlSchemaForm.Unqualified)]
        public required string LastName { get; init; } = String.Empty;

        /// <summary>
        /// Gets the email address associated with this instance.
        /// </summary>
        [XmlElement("EMail", Form = XmlSchemaForm.Unqualified)]
        public required string EMail { get; init; } = String.Empty;

        /// <summary>
        /// Gets the title associated with this instance.
        /// </summary>
        [XmlElement("Title", Form = XmlSchemaForm.Unqualified)]
        public required string Title { get; init; } = String.Empty;

        public ActivationRequest() { }
    }
}
