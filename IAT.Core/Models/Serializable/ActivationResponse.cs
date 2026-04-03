using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Models.Enumerations;

namespace IAT.Core.Models.Serializable
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
        public ProductActivationResult ActivationResult { get; set; } = ProductActivationResult.Unset;

        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
        private string ProductKey { get; set; }
        
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
        private string VerificationCode { get; set; }

        [XmlElement("ClientName", Form = XmlSchemaForm.Unqualified)]
        private string Name { get; set; }

        [XmlElement("ClientEMail", Form = XmlSchemaForm.Unqualified)]
        private string EMail { get; set; }

        [XmlElement("Phone", Form = XmlSchemaForm.Unqualified)]
        private string Phone { get; set; }

        [XmlElement("Address1", Form = XmlSchemaForm.Unqualified)]
        private string Address1 { get; set; }

        [XmlElement("Address2", Form = XmlSchemaForm.Unqualified)]
        private String Address2 { get; set; }

        [XmlElement("City", Form = XmlSchemaForm.Unqualified)]
        private string City { get; set; }

        [XmlElement("Province", Form = XmlSchemaForm.Unqualified)]
        private string Province{ get; set; }

        [XmlElement("PostalCode", Form = XmlSchemaForm.Unqualified)]
        private string PostalCode { get; set; }

        [XmlElement("Country", Form = XmlSchemaForm.Unqualified)]
        private string Country { get; set; }    


        public ActivationResponse() { }
    }
}
