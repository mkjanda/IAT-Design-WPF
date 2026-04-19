using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents the response returned from a product activation request, including the activation result and related
    /// client information.
    /// </summary>
    /// <remarks>This class is typically used to encapsulate the outcome of a product activation operation,
    /// providing both the result and associated client details as part of the response. Only the activation result is
    /// exposed publicly; other client and product information is managed internally.</remarks>
    public class ActivationResponse 
    {

        /// <summary>
        /// Specifies the possible outcomes of a client activation request.
        /// </summary>
        /// <remarks>Use this enumeration to determine the result of an activation operation, such as
        /// whether the activation was successful, failed due to a specific error, or could not be completed for another
        /// reason. Each value represents a distinct result that callers can use to handle activation logic
        /// appropriately.</remarks>

        [XmlElement("AxtivationResult", Form = XmlSchemaForm.Unqualified)]
        public TransactionResult TransactionResult { get; set; } = TransactionResult.Unset;

        /// <summary>
        /// Gets or sets the product key associated with the current instance.
        /// </summary>
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
        private string ProductKey { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the verification code associated with the product key.
        /// </summary>
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
        private string VerificationCode { get; set; } = String.Empty;   

        /// <summary>
        /// Gets or sets the client name associated with this instance.
        /// </summary>
        [XmlElement("ClientName", Form = XmlSchemaForm.Unqualified)]
        private string Name { get; set; } = String.Empty;

        [XmlElement("ClientEMail", Form = XmlSchemaForm.Unqualified)]
        private string EMail { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the phone number associated with this instance.
        /// </summary>
        [XmlElement("Phone", Form = XmlSchemaForm.Unqualified)]
        private string Phone { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the first line of the street address.
        /// </summary>
        [XmlElement("Address1", Form = XmlSchemaForm.Unqualified)]
        private string Address1 { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the secondary address line for the location.
        /// </summary>
        [XmlElement("Address2", Form = XmlSchemaForm.Unqualified)]
        private String Address2 { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the name of the city.
        /// </summary>
        [XmlElement("City", Form = XmlSchemaForm.Unqualified)]
        private string City { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the province associated with the entity.
        /// </summary>
        [XmlElement("Province", Form = XmlSchemaForm.Unqualified)]
        private string Province{ get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the postal code associated with the address.
        /// </summary>
        [XmlElement("PostalCode", Form = XmlSchemaForm.Unqualified)]
        private string PostalCode { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the country associated with the current entity.
        /// </summary>
        [XmlElement("Country", Form = XmlSchemaForm.Unqualified)]
        private string Country { get; set; } =  String.Empty;

        /// <summary>
        /// Initializes a new instance of the ActivationResponse class.
        /// </summary>
        public ActivationResponse() { }
    }
}
