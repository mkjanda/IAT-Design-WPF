using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using IAT.Core.ConfigFile;
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
        private TaskCompletionSource<TransactionResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Gets a task that represents the completion of the transaction.
        /// </summary>
        public Task<TransactionResult> Completion => _completion.Task;

        /// <summary>
        /// Sets the result of the transaction operation and completes <see cref="Completion"/>.
        /// </summary>
        /// <param name="result">The transaction result to set.</param>
        public void SetResult(TransactionResult result)
        {
            Result = result;
            _completion.TrySetResult(result);
        }

        /// <summary>
        /// Transitions the underlying task to a canceled state.
        /// </summary>
        public void SetCanceled()
        {
            _completion.TrySetCanceled();
        }

        /// <summary>
        /// Sets the exception that caused the transaction to fail.
        /// </summary>
        /// <param name="ex">The exception to set.</param>
        public void SetException(Exception ex)
        {
            _completion.TrySetException(ex);
        }

        /// <summary>
        /// Resets the completion task to allow a new asynchronous operation to be awaited.
        /// </summary>
        public void ResetCompletion()
        {
            // New TCS for the next transaction
            _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Gets or sets the product key associated with the product.
        /// </summary>
        public string ProductKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password used for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authentication token used for securing transactions.
        /// </summary>
        public string AuthToken { get; set; } = string.Empty;

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
        /// The configuration file for the test being deployed. This property is set during the deployment
        /// process and contains the necessary configuration details for the test, such as stimuli, instructions,
        /// and other relevant settings. The ConfigFile is typically used to initialize the test environment and
        /// ensure that the test is configured correctly according to the specifications defined in the configuration file.
        /// </summary>
        public IATConfigFile ConfigFile { get; set; } = new IATConfigFile();

        /// <summary>
        /// The manifest of deployment files.
        /// </summary>
        public Manifest FileManifest { get; set; } = new Manifest();

        /// <summary>
        /// Gets or sets the manifest that defines the structure and metadata for the slide.
        /// </summary>
        public Manifest SlideManifest { get; set; } = new Manifest();

        /// <summary>
        /// Gets or sets the number of results associated with the transaction. This property is used to track
        /// the count of results generated or processed during the transaction operation. It is represented as
        /// an integer value and can be used for validation, reporting, or further processing of the results.
        /// The NumResults property helps maintain an accurate record of the results produced in the context
        /// of the transaction workflow.
        /// </summary>
        public int NumResults { get; set; } = 0;

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
        /// The deployment ID of the test upload.
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
        /// Gets or sets the server report containing information about the server's response to the transaction.
        /// </summary>
        public ServerReport ServerReport { get; set; } = new ServerReport();

        /// <summary>
        /// Gets or sets the test results.
        /// </summary>
        public TestResults TestResults { get; set; } = new TestResults();

        /// <summary>
        /// Resets all user and session-related properties to their default values and arms a fresh
        /// <see cref="Completion"/> task for the next transaction.
        /// </summary>
        /// <remarks>Call this method to clear sensitive information and restore the object to its initial
        /// state before reuse.</remarks>
        public void Clear()
        {
            ProductKey = string.Empty;
            Password = string.Empty;
            AuthToken = string.Empty;
            IATName = string.Empty;
            UserName = string.Empty;
            Email = string.Empty;
            TestResultsDocument = new XDocument();
            SlideManifest = new Manifest();
            FileManifest = new Manifest();
            RSA = new EncryptedRSAKey();
            Result = TransactionResult.Unset;
            ActivationKey = string.Empty;
            ServerReport = new ServerReport();
            TestResults = new TestResults();
            NumResults = 0;
            ClientId = 0;
            DeploymentId = 0;
            UploadTimeMillis = 0;
            ResetCompletion();
        }
    }
}
