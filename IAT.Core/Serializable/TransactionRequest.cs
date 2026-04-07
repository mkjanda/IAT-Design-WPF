using IAT.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel.Design.Serialization;
using IAT.Core.Enumerations;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Specifies the set of transaction and operation codes used to represent various requests, responses, and
    /// status indicators within the system.
    /// </summary>
    /// <remarks>The ETransaction enumeration defines values for identifying specific actions, events,
    /// and states related to transactions, client operations, authentication, deployment, and error conditions.
    /// These codes are typically used for communication between system components or for interpreting the results
    /// of operations. The meaning and usage of each value are context-dependent; refer to the documentation for
    /// each member for details on its purpose and appropriate scenarios for use.</remarks>
    public enum TransactionType
    {
        /// <summary>
        /// Represents a default or uninitialized value for the associated enumeration.
        /// </summary>
        /// <remarks>Use this value to indicate that no explicit value has been set. This is
        /// typically used as a sentinel value in enumerations.</remarks>
        Unset,

        /// <summary>
        /// Specifies the available modes for transmitting a request.
        /// </summary>
        /// <remarks>Use this enumeration to indicate how a request should be sent, such as
        /// synchronously or asynchronously. The selected mode may affect performance and response
        /// handling.</remarks>
        RequestTransmission,

        /// <summary>
        /// Aborts the current transaction, rolling back any changes made since the transaction began.
        /// </summary>
        /// <remarks>Call this method to cancel a transaction and undo all operations performed
        /// within it. After aborting, the transaction cannot be resumed or committed.</remarks>
        AbortTransaction,

        /// <summary>
        /// Deletes the IAT (Implicit Assoociation Test) from the server
        /// </summary>
        DeleteIAT,

        /// <summary>
        /// Deletes all data associated with the Implicit Association Test (IAT).
        /// </summary>
        /// <remarks>Use this method to remove IAT data when it is no longer needed or to reset
        /// the test state. This operation is irreversible and will permanently remove all related
        /// records.</remarks>
        DeleteIATData,

        /// <summary>
        /// Gets or sets a value indicating whether the transaction completed successfully.
        /// </summary>
        TransactionSuccess,

        /// <summary>
        /// Indicates that a transaction has failed.
        /// </summary>
        TransactionFail,

        /// <summary>
        /// Represents a value indicating whether the specified entity exists in the system.
        /// </summary>
        IATExists,

        /// <summary>
        /// request the encrypted RSA keypair for an IAT
        /// </summary>
        RequestEncryptionKey,

        /// <summary>
        /// Represents a request to verify a user's password.
        /// </summary>
        RequestPasswordVerification,

        /// <summary>
        /// Verifies whether the specified password meets the required criteria or matches a stored credential.
        /// </summary>
        VerifyPassword,

        /// <summary>
        /// Represents an error condition where the specified IAT (Import Address Table) does not exist.
        /// </summary>
        NoSuchIAT,

        /// <summary>
        /// Indicates that the client is in a frozen state and cannot perform operations.
        /// </summary>
        ClientFrozen,

        /// <summary>
        /// Represents an event that occurs when a client is deleted.
        /// </summary>
        ClientDeleted,

        /// <summary>
        /// Represents a request to retrieve a slide manifest item.
        /// </summary>
        RequestItemSlideManifest,

        /// <summary>
        /// Requests that a verification email be sent to the user's registered email address.
        /// </summary>
        /// <remarks>Use this method to initiate the email verification process for a user
        /// account. The user will receive an email containing instructions or a link to verify their email address.
        /// This is typically used during account registration or when an email address change needs to be
        /// confirmed.</remarks>
        RequestEMailVerification,

        /// <summary>
        /// Requests that a new verification email be sent to the user.
        /// </summary>
        /// <remarks>Use this method to trigger the delivery of a new verification email when the
        /// user has not yet completed email verification. This is typically used in account registration or
        /// recovery scenarios.</remarks>
        RequestNewVerificationEMail,

        /// <summary>
        /// Represents an error condition indicating that the specified client does not exist.
        /// </summary>
        NoSuchClient,

        /// <summary>
        /// Indicates that the email address has already been verified.
        /// </summary>
        EMailAlreadyVerified,

        /// <summary>
        /// Gets or sets a value indicating whether the server report should be requested.
        /// </summary>
        RequestServerReport,

        /// <summary>
        /// Indicates that an operation failed due to insufficient disk space.
        /// </summary>
        InsufficientDiskSpace,

        /// <summary>
        /// Represents an error condition indicating that there are insufficient IATS (Inter-Account Transfer
        /// System) resources or capacity to complete the requested operation.
        /// </summary>
        InsufficientIATS,

        /// <summary>
        /// Represents a request to establish a connection.
        /// </summary>
        RequestConnection,

        /// <summary>
        /// Represents the results of a request operation, including status and any associated data.
        /// </summary>
        RequestResults,

        /// <summary>
        /// Represents a request to initiate an IAT (Import Authorization Token) upload operation.
        /// </summary>
        RequestIATUpload,

        /// <summary>
        /// Gets or sets a value indicating whether the password meets the required validation criteria.
        /// </summary>
        PasswordValid,

        /// <summary>
        /// Indicates that the provided password is invalid.
        /// </summary>
        PasswordInvalid,

        /// <summary>
        /// Represents a request to retrieve a collection of slides from a presentation.
        /// </summary>
        RequestItemSlides,

        /// <summary>
        /// Represents an error state indicating that a backup cannot be restored.
        /// </summary>
        CannotRestoreBackup,

        /// <summary>
        /// Gets or sets a value indicating whether a backup has been successfully restored.
        /// </summary>
        BackupRestored,

        /// <summary>
        /// Represents a request to initiate an IAT (Initial Access Token) redeployment operation.
        /// </summary>
        RequestIATRedeploy,

        /// <summary>
        /// Gets the number of remaining IATS (Immediate Access Tokens) available for use.
        /// </summary>
        QueryRemainingIATS,

        /// <summary>
        /// Gets or sets the number of remaining IATS (Immediate Access Transactions) available.
        /// </summary>
        RemainingIATS,

        /// <summary>
        /// Represents a request to upload data.
        /// </summary>
        RequestDataUpload,

        /// <summary>
        /// Gets or sets a value indicating whether the test is currently being deployed.
        /// </summary>
        TestBeingDeployed,

        /// <summary>
        /// Halts the currently running test deployment process.
        /// </summary>
        /// <remarks>Use this method to stop an active test deployment before it completes. This
        /// can be useful for aborting tests in response to errors or user intervention. The state of the deployment
        /// after halting may be incomplete or require cleanup.</remarks>
        HaltTestDeployment,

        /// <summary>
        /// Indicates whether the deployment process has been halted.
        /// </summary>
        DeploymentHalted,

        /// <summary>
        /// Gets or sets a value indicating whether the item slide download is ready for processing.
        /// </summary>
        ItemSlideDownloadReady,

        /// <summary>
        /// Specifies an error that occurs when a deployment descriptor does not match the expected configuration.
        /// </summary>
        DeploymentDescriptorMismatch,

        /// <summary>
        /// Gets or sets a value indicating whether the encryption keys have been received.
        /// </summary>
        EncryptionKeysReceived,

        /// <summary>
        /// Gets or sets the deployment file manifest that was received.
        /// </summary>
        DeploymentFileManifestReceived,

        /// <summary>
        /// Gets or sets the manifest data received for the item slide.
        /// </summary>
        ItemSlideManifestReceived,

        /// <summary>
        /// Gets or sets the token definition that was received.
        /// </summary>
        TokenDefinitionReceived,

        /// <summary>
        /// Initiates a handshake request with a remote endpoint to establish communication or verify connectivity.
        /// </summary>
        RequestHandshake,

        /// <summary>
        /// Aborts the current deployment process, stopping any ongoing operations and rolling back changes if
        /// possible.
        /// </summary>
        /// <remarks>Use this method to halt a deployment that is in progress. Depending on the
        /// deployment system, aborting may leave the environment in a partially updated state. Ensure that aborting
        /// is appropriate for your scenario before calling this method.</remarks>
        AbortDeployment,

        /// <summary>
        /// Gets a value indicating whether one or more required test files are missing.
        /// </summary>
        TestFilesMissing,

        /// <summary>
        /// Indicates that an attempt to abort the deployment has failed.
        /// </summary>
        DeploymentAbortFailed,

        /// <summary>
        /// Gets a value indicating whether the results are ready for retrieval.
        /// </summary>
        ResultsReady,

        /// <summary>
        /// Indicates that no activations remain for the current entity or operation.
        /// </summary>
        NoActivationsRemain,

        /// <summary>
        /// Represents an error condition where the provided email address does not match the expected value during
        /// verification.
        /// </summary>
        EmailVerificationMismatch
    };



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
    public class TransactionRequest 
    {

        /// <summary>
        /// Gets the collection of integer values associated with their corresponding string keys.
        /// </summary>
        /// <remarks>The returned dictionary is read-only from outside the class. Modifications to the
        /// collection can only be made internally.</remarks>
        [XmlIgnore]
        public Dictionary<String, int> IntValues { get; private set; } = new();

        /// <summary>
        /// Represents a serializable key-value pair with a string key and an integer value.
        /// </summary>
        /// <remarks>This structure is typically used for scenarios where integer values need to be
        /// associated with string keys and serialized, such as in XML-based data exchange. The structure is decorated
        /// with XML serialization attributes to control the element names during serialization.</remarks>
        public struct IntValueSerializable
        {
            [XmlElement("Key")]
            public String Key { get; set; }
            [XmlElement("Value")]
            public int Value { get; set; }
        }   

        /// <summary>
        /// Gets or sets the collection of integer key-value pairs in a format suitable for XML serialization.
        /// </summary>
        /// <remarks>This property is intended for use with XML serialization frameworks. Modifying this
        /// property updates the underlying collection of integer values, and retrieving it provides a serializable
        /// representation of the current state.</remarks>
        [XmlArray("IntValuesSerializable")]
        [XmlArrayItem("IntValue")]
        public List<IntValueSerializable> IntValuesSerializable
        {
            get
            {
                List<IntValueSerializable> list = new List<IntValueSerializable>();
                foreach (String key in IntValues.Keys)
                {
                    list.Add(new IntValueSerializable() { Key = key, Value = IntValues[key] });
                }
                return list;
            }
            set
            {
                if (value != null)
                {
                    IntValues.Clear();
                    foreach (IntValueSerializable item in value)
                    {
                        IntValues[item.Key] = item.Value;
                    }
                }
            }
        }



        /// <summary>
        /// Gets a collection of key/value pairs where each key is a string and each value is a long integer.
        /// </summary>
        [XmlIgnore]
        public Dictionary<String, long> LongValues { get; private set; } = new();

        /// <summary>
        /// Represents a serializable key-value pair with a string key and a 64-bit integer value.
        /// </summary>
        /// <remarks>This structure is intended for scenarios where a long integer value needs to be
        /// associated with a string key and serialized, such as in XML-based data exchange. The structure uses XML
        /// element attributes to control serialization of its members.</remarks>
        public struct LongValueSerializable
        {
            [XmlElement("Key")]
            public String Key { get; set; }
            [XmlElement("Value")]
            public long Value { get; set; }
        }

        /// <summary>
        /// Gets or sets the collection of long values in a format suitable for XML serialization.
        /// </summary>
        /// <remarks>This property provides a serializable representation of the underlying long values
        /// dictionary. When setting this property, the existing collection is replaced with the provided values. The
        /// property is primarily intended for use with XML serialization frameworks.</remarks>
        [XmlArray("LongValuesSerializable")]
        [XmlArrayItem("LongValue")] 
        public List<LongValueSerializable> LongValuesSerializable
        {
            get
            {
                List<LongValueSerializable> list = new List<LongValueSerializable>();
                foreach (String key in LongValues.Keys)
                {
                    list.Add(new LongValueSerializable() { Key = key, Value = LongValues[key] });
                }
                return list;
            }
            set
            {
                if (value != null)
                {
                    LongValues.Clear();
                    foreach (LongValueSerializable item in value)
                    {
                        LongValues[item.Key] = item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the collection of string key-value pairs associated with this instance.
        /// </summary>
        /// <remarks>The dictionary provides access to all string-based values relevant to the current
        /// context. Keys are case-sensitive. Modifications to the returned dictionary affect the internal state of the
        /// instance.</remarks>
        [XmlIgnore]
        public Dictionary<String, String> StringValues { get; private set; } = new();

        /// <summary>
        /// Represents a serializable key-value pair with both key and value as strings.
        /// </summary>
        /// <remarks>This struct is typically used for XML serialization scenarios where simple
        /// string-based key-value pairs need to be represented in a structured format. The struct is decorated with XML
        /// element attributes to control the XML output during serialization.</remarks>
        public struct StringValueSerializable
        {
            [XmlElement("Key")]
            public String Key { get; set; }
            [XmlElement("Value")]
            public String Value { get; set; }
        }       

        /// <summary>
        /// Gets or sets the collection of string key-value pairs in a serializable format.
        /// </summary>
        /// <remarks>This property provides access to the string values as a list of serializable objects,
        /// suitable for XML serialization. Setting this property replaces the current collection with the provided
        /// values. The order of items in the list is not guaranteed to be preserved.</remarks>
        [XmlArray("StringValuesSerializable")]
        [XmlArrayItem("StringValue")]
        public List<StringValueSerializable> StringValuesSerializable
        {
            get
            {
                List<StringValueSerializable> list = new List<StringValueSerializable>();
                foreach (String key in StringValues.Keys)
                {
                    list.Add(new StringValueSerializable() { Key = key, Value = StringValues[key] });
                }
                return list;
            }
            set
            {
                if (value != null)
                {
                    StringValues.Clear();
                    foreach (StringValueSerializable item in value)
                    {
                        StringValues[item.Key] = item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the transaction type associated with the current operation.
        /// </summary>
        [XmlElement("Transaction")]
        public TransactionType Transaction { get; set; }

        /// <summary>
        /// Gets or sets the activation key used to enable or validate the product or feature.
        /// </summary>
        [XmlElement("ActivationKey")]
        public String ActivationKey { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the name of the IAT (Item Analysis Tool) instance.
        /// </summary>
        [XmlElement("IATName")]
        public String IATName { get; set; } = String.Empty; 

        /// <summary>
        /// Gets the unique identifier for the client.
        /// </summary>
        [XmlElement("ClientID")]
        public int ClientID { get; private set; }

        /// <summary>
        /// Gets or sets the product key used to activate or identify the product.
        /// </summary>
        [XmlElement("ProductKey")]
        public String ProductKey { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this transaction is the last in a sequence of transactions.
        /// </summary>
        [XmlElement("IsLastTransaction")]
        public bool IsLastTransaction { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the TransactionRequest class using the specified local storage service.
        /// </summary>
        /// <param name="localStorage">The local storage service used to retrieve product and activation keys. Cannot be null.</param>
        public TransactionRequest()
        {
            Transaction = TransactionType.Unset;
            IATName = String.Empty;
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
            Transaction = TransactionType.Unset;
            IATName = String.Empty;
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
            Transaction = tType;
            IATName = String.Empty;
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
            Transaction = tType;
            this.IATName = IATName;
            ProductKey = localStorage[Field.ProductKey];
            ActivationKey = localStorage[Field.ActivationKey];
        }

    }
}

