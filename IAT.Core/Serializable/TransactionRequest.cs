using IAT.Core.Services;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;
using MediatR;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents a request for a transaction operation, including transaction type, associated data, and relevant
    /// identifiers.
    /// </summary>
    /// <remarks>The TransactionRequest class encapsulates all information required to perform or describe a
    /// transaction within the system. It provides properties for specifying the transaction type, client and product
    /// identifiers, and collections for additional data. Instances are typically constructed with a local storage
    /// service to retrieve necessary keys. This class is used as a data contract for communication between system
    /// components or services.</remarks>
    [XmlRoot("TransactionRequest")]
    public class TransactionRequest : IWebSocketMessage
    {

        /// <summary>
        /// Gets or sets the transaction type associated with the current operation.
        /// </summary>
        [XmlElement("Type", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public TransactionType Type { get; set; }


        /// <summary>
        /// Gets or sets the product key used to activate or identify the product.
        /// </summary>
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public String ProductKey { get; set; } = String.Empty;


        /// <summary>
        /// Gets or sets the activation key used to enable or validate the product or feature.
        /// </summary>
        [XmlElement("ActivationKey", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public String ActivationKey { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the name of the IAT (Item Analysis Tool) instance.
        /// </summary>
        [XmlElement("IATName", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public String IATName { get; set; } = String.Empty; 

        /// <summary>
        /// Gets the unique identifier for the client.
        /// </summary>
        [XmlElement("ClientId", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public long ClientId { get; set; }


        /// <summary>
        /// Gets or sets the unique identifier for the deployment associated with this transaction. This property 
        /// is nullable to allow for cases where a deployment ID may not be applicable or available.
        /// </summary>
        [XmlElement("DeploymentId", Form = XmlSchemaForm.Unqualified)]
        public long DeploymentId { get; set; } = default;

        /// <summary>
        /// Gets or sets the start time of the deployment associated with this transaction, represented as a long value.
        /// </summary>
        [XmlElement("DeploymentStartTime", Form = XmlSchemaForm.Unqualified)]
        public long DeploymentStartTime { get; set; } = default;

        /// <summary>
        /// Gets or sets the user name associated with this transaction. This property is nullable to 
        /// accommodate scenarios where a user name may not be provided.
        /// </summary>
        [XmlElement("Email", Form = XmlSchemaForm.Unqualified)]
        public string Email { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the authentication token associated with this transaction. This property is nullable to 
        /// accommodate scenarios where an authentication token may not be provided.
        /// </summary>
        [XmlElement("AuthToken", Form = XmlSchemaForm.Unqualified)]
        public string AuthToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the encrypted test string associated with this transaction. This property is nullable to 
        /// accommodate scenarios where an encrypted test string may not be provided.
        /// </summary>
        [XmlElement("TestString", Form = XmlSchemaForm.Unqualified)]
        public string TestString { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets a value indicating whether this transaction is the last in a sequence of transactions.
        /// </summary>
        [XmlElement("IsLastTransaction", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public bool IsLastTransaction { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the TransactionRequest class with default values.
        /// </summary>
        public TransactionRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TransactionRequest class using values from the specified local storage
        /// service.
        /// </summary>
        /// <remarks>The constructor sets the Transaction property to Unset and initializes the IATName
        /// property to an empty string. ProductKey and ActivationKey are loaded from the provided local storage
        /// service.</remarks>
        /// <param name="localStorage">The local storage service used to retrieve the product and activation keys. Cannot be null.</param>
        public TransactionRequest(ILocalStorageService localStorage)
        {
            ProductKey = localStorage[Field.ProductKey];
            ActivationKey = localStorage[Field.ActivationKey];
        }

        /// <summary>
        /// Initializes a new instance of the TransactionRequest class with the specified transaction type and local
        /// storage service.
        /// </summary>
        /// <param name="tType">The type of transaction to be performed.</param>
        /// <param name="localStorage">The local storage service used to retrieve product and activation keys. Cannot be null.</param>
        public TransactionRequest(TransactionType tType, ILocalStorageService localStorage)
        {
            ProductKey = localStorage[Field.ProductKey];
            ActivationKey = localStorage[Field.ActivationKey];
        }

        /// <summary>
        /// Initializes a new instance of the TransactionRequest class with the specified transaction type, IAT name,
        /// and local storage service.
        /// </summary>
        /// <param name="tType">The type of transaction to be performed.</param>
        /// <param name="IATName">The name of the IAT instance associated with this transaction.</param>
        /// <param name="localStorage">The local storage service used to retrieve product and activation keys.</param>
        public TransactionRequest(TransactionType tType, String IATName, ILocalStorageService localStorage)
        {
            Type = tType;
            this.IATName = IATName;
            ProductKey = localStorage[Field.ProductKey];
            ActivationKey = localStorage[Field.ActivationKey];
        }

    }
}

