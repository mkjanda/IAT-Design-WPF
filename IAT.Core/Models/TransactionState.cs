using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using sun.reflect.generics.tree;

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
        /// Gets or sets the unique identifier for the client.
        /// </summary>
        public long ClientId { get; set; } = 0;

        /// <summary>
        /// The deployment ID of the test upload
        /// </summary>
        public long DeploymentId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the upload completion time in milliseconds since the Unix epoch.
        /// </summary>
        public long UploadTimeMillis { get; set; } = 0;

        /// <summary>
        /// The activation key associated with the current activation or verification process. This property is updated upon successful email 
        /// verification or product activation and can be used to retrieve the activation key for storage or display purposes.
        /// </summary>
        public string ActivationKey { get; set; } = string.Empty;

        /// <summary>
        /// The event that signals the completion of a transaction operation. This ManualResetEvent is used to synchronize the flow of the 
        /// transaction process, allowing the calling code to wait for the transaction to complete before proceeding. When the transaction is 
        /// completed, the event is set, allowing any waiting threads to continue execution. This is particularly useful in asynchronous 
        /// operations where the transaction may involve network communication or other time-consuming tasks, ensuring that the application 
        /// remains responsive while waiting for the transaction to complete. 
        /// </summary>
        public ManualResetEvent Event { get; } = new ManualResetEvent(false);

        /// <summary>
        /// Resets all user and session-related properties to their default values.
        /// </summary>
        /// <remarks>Call this method to clear sensitive information and restore the object to its initial
        /// state before reuse. This method also signals any waiting threads that the reset operation has
        /// completed.</remarks>
        public void Clear()
        {
            ProductKey = string.Empty;
            Password = string.Empty;
            IATName = string.Empty;
            UserName = string.Empty;
            Email = string.Empty;
            TestResultsDocument = new XDocument();
            SlideManifest = new Manifest();
            RSA = new EncryptedRSAKey();
            Result = TransactionResult.Unset;
            ActivationKey = string.Empty;
            Event.Set();
        }
    }
}
