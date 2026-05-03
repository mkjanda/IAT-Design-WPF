using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;

namespace IAT.Core.Models
{
    /// <summary>
    /// Represents the state and related data for a transaction, including user information, product details, test
    /// results, and cryptographic keys.
    /// </summary>
    /// <remarks>This class is used to encapsulate all relevant information required during a transaction
    /// process, such as activation or verification. It provides properties for storing user credentials, product keys,
    /// test results, and security-related data. The class is typically used as a data container throughout the
    /// transaction workflow.</remarks>
    public class TransactionState
    {
        /// <summary>
        /// Gets or sets the product key associated with the product.
        /// </summary>
        public string ProductKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password used for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the IAT (Implicit Association Test) associated with this instance.
        /// </summary>
        public string IATName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user name associated with the current instance.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address associated with the entity.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the XML document that contains the test results.
        /// </summary>
        public XDocument TestResultsDocument { get; set; } = new();

        /// <summary>
        /// Gets or sets the manifest that defines the structure and metadata for the slide.
        /// </summary>
        public Manifest SlideManifest { get; set; } = new();

        /// <summary>
        /// Gets or sets the RSA key information used for encryption operations.
        /// </summary>
        public EncryptedRSAKey RSA { get; set; } = new();

        /// <summary>
        /// Gets or sets the result of the transaction operation.
        /// </summary>
        public TransactionResult Result { get; set; } = TransactionResult.Unset;

        /// <summary>
        /// The activation key associated with the current activation or verification process. This property is updated upon successful email 
        /// verification or product activation and can be used to retrieve the activation key for storage or display purposes.
        /// </summary>
        public string ActivationKey { get; set; } = string.Empty;
    }
}
