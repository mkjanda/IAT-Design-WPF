using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.ConfigFile;
using IAT.Core.Enumerations;
using MediatR;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents a command that encapsulates a ResultSetDescriptor object, which contains metadata and 
    /// configuration information related to the results of a test or assessment. This command is used to 
    /// transmit the result set descriptor to the appropriate handler for processing.
    /// </summary>
    /// <param name="response"></param>
    public record ResultSetDescriptorCommand(ResultSetDescriptor response) : IRequest<TransactionResult>;


    /// <summary>
    /// Represents a descriptor for a result set, containing metadata and configuration information related to the results of a test or assessment.
    /// </summary>
    [XmlRoot("ResultSetDescriptor")]
    public class ResultSetDescriptor : IWebSocketMessage
    {
        /// <summary>
        /// Gets or sets the product key associated with the result set descriptor. This key is used to identify the product or application for 
        /// which the results are being generated.
        /// </summary>
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)] 
        public string ProductKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the test associated with the result set descriptor. This name is used to identify the specific test or assessment
        /// </summary>
        [XmlElement("TestAuthor", Form = XmlSchemaForm.Unqualified)] 
        public string TestAuthor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author of the test associated with the result set descriptor. This information is used to 
        /// identify the individual or organization
        /// </summary>
        [XmlElement("ConfigFile", Form = XmlSchemaForm.Unqualified, Type=typeof(IATConfigFile))] 
        public IATConfigFile ConfigFile { get; set; } = new IATConfigFile();

        /// <summary>
        /// Gets or sets the number of results in the result set.
        /// </summary>
        [XmlElement("NumResults", Form = XmlSchemaForm.Unqualified)]
        public int NumResults { get; set; } = 0;

        /// <summary>
        /// Gets or sets the RSA key used for encrypting the result set.
        /// </summary>
        [XmlElement("RSAKey", Form = XmlSchemaForm.Unqualified, Type = typeof(EncryptedRSAKey))]
        public EncryptedRSAKey RSAKey { get; set; } = new EncryptedRSAKey();
    }
}

