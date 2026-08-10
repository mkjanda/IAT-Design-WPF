using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Specifies the types of transactions that can occur within the system, including activation, deployment, and verification operations.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Represents a default or uninitialized value for the associated enumeration.
        /// </summary>
        /// <remarks>Use this value to indicate that no explicit value has been set. This is
        /// typically used as a sentinel value in enumerations.</remarks>
        [Description("Unset")]
        Unset,

        /// <summary>
        /// Specifies the available modes for transmitting a request.
        /// </summary>
        /// <remarks>Use this enumeration to indicate how a request should be sent, such as
        /// synchronously or asynchronously. The selected mode may affect performance and response
        /// handling.</remarks>
        [Description("Request Transmission")]
        RequestTransmission,

        /// <summary>
        /// Indicates that a connection request is being made to establish communication with a remote endpoint.
        /// </summary>
        [Description("Request Connection")]
        RequestConnection,

        /// <summary>
        /// Aborts the current transaction, rolling back any changes made since the transaction began.
        /// </summary>
        /// <remarks>Call this method to cancel a transaction and undo all operations performed
        /// within it. After aborting, the transaction cannot be resumed or committed.</remarks>
        [Description("Abort Transaction")]
        AbortTransaction,

        /// <summary>
        /// Deletes the IAT (Implicit Assoociation Test) from the server
        /// </summary>
        [Description("Delete IAT")]
        DeleteIAT,

        /// <summary>
        /// Deletes all data associated with the Implicit Association Test (IAT).
        /// </summary>
        /// <remarks>Use this method to remove IAT data when it is no longer needed or to reset
        /// the test state. This operation is irreversible and will permanently remove all related
        /// records.</remarks>
        [Description("Delete IAT Data")]
        DeleteIATData,

        /// <summary>
        /// Gets or sets a value indicating whether the transaction completed successfully.
        /// </summary>
        [Description("Transaction Success")]
        Success,

        /// <summary>
        /// Indicates that a transaction has failed.
        /// </summary>
        [Description("Transaction Fail")]
        Fail,

        /// <summary>
        /// Represents a value indicating whether the specified entity exists in the system.
        /// </summary>
        [Description("An IAT with the specified name already exists.")]
        IATExists,

        /// <summary>
        /// request the encrypted RSA keypair for an IAT
        /// </summary>
        [Description("Request Encryption Key")]
        RequestEncryptionKey,

        /// <summary>
        /// Gets or sets the encryption key that was received.
        /// </summary>
        [Description("EncrptionKeyWasReceived")]
        EncryptionKeyReceived,

        /// <summary>
        /// Represents a request to verify a user's password.
        /// </summary>
        [Description("Request that that IAT password be verified")]
        RequestPasswordVerification,

        /// <summary>
        /// Verifies whether the specified password meets the required criteria or matches a stored credential.
        /// </summary>
        [Description("The transaction that contains the password verificsation information")]
        VerifyPassword,

        /// <summary>
        /// Represents an error condition where the specified IAT (Import Address Table) does not exist.
        /// </summary>
        [Description("No IAT with that name exists on the server.")]
        NoSuchIAT,

        /// <summary>
        /// Represents a request to retrieve a slide manifest item.
        /// </summary>
        [Description("The item slide manifest is being requested.")]
        RequestItemSlideManifest,

        /// <summary>
        /// Requests that a verification email be sent to the user's registered email address.
        /// </summary>
        /// <remarks>Use this method to initiate the email verification process for a user
        /// account. The user will receive an email containing instructions or a link to verify their email address.
        /// This is typically used during account registration or when an email address change needs to be
        /// confirmed.</remarks>
        [Description(("Email verifiication is being requested"))]
        RequestEMailVerification,

        /// <summary>
        /// Requests that a new verification email be sent to the user.
        /// </summary>
        /// <remarks>Use this method to trigger the delivery of a new verification email when the
        /// user has not yet completed email verification. This is typically used in account registration or
        /// recovery scenarios.</remarks>
        [Description("A new verification email is being requested")]
        RequestNewVerificationEMail,

        /// <summary>
        /// Represents an error condition indicating that the specified client does not exist.
        /// </summary>
        [Description("No client with that product key")]
        NoSuchClient,

        /// <summary>
        /// Indicates that the email address has already been verified.
        /// </summary>
        [Description("That email address has been verified")]
        EMailAlreadyVerified,

        /// <summary>
        /// Gets or sets a value indicating whether the server report should be requested.
        /// </summary>
        [Description("The server report for a client is being requested")]
        RequestServerReport,

        /// <summary>
        /// Indicates that an operation failed due to insufficient disk space.
        /// </summary>
        [Description("Insufficient disk space exists to deploy that IAT.")]
        InsufficientDiskSpace,

        /// <summary>
        /// Represents the results of a request operation, including status and any associated data.
        /// </summary>
        [Description("The results of a request operation, including status and any associated data")]
        RequestResults,

        /// <summary>
        /// Represents a request to initiate an IAT (Import Authorization Token) upload operation.
        /// </summary>
        [Description("An IAT (Import Authorization Token) upload operation is being requested")]
        RequestIATUpload,

        /// <summary>
        /// Gets or sets a value indicating whether the password meets the required validation criteria.
        /// </summary>
        [Description("Indicates whether the provided password is valid")]
        PasswordValid,

        /// <summary>
        /// Indicates that the provided password is invalid.
        /// </summary>
        [Description("Indicates that the provided password is invalid")]
        PasswordInvalid,

        /// <summary>
        /// Represents a request to retrieve a collection of slides from a presentation.
        /// </summary>
        [Description("A request to retrieve a collection of slides from a presentation is being made")]
        RequestItemSlides,

        /// <summary>
        /// Represents an error state indicating that a backup cannot be restored.
        /// </summary>
        [Description("An error occurred indicating that a backup cannot be restored")]
        CannotRestoreBackup,

        /// <summary>
        /// Gets or sets a value indicating whether a backup has been successfully restored.
        /// </summary>
        [Description("Indicates whether a backup has been successfully restored")]
        BackupRestored,

        /// <summary>
        /// Represents a request to initiate an IAT (Initial Access Token) redeployment operation.
        /// </summary>
        [Description("An IAT (Initial Access Token) redeployment operation is being requested")]
        RequestIATRedeploy,

        /// <summary>
        /// Gets the number of remaining IATS (Immediate Access Tokens) available for use.
        /// </summary>
        [Description("Gets the number of remaining IATS (Immediate Access Tokens) available for use")]
        QueryRemainingIATS,

        /// <summary>
        /// Gets or sets the number of remaining IATS (Immediate Access Transactions) available.
        /// </summary>
        [Description("Returns the number of remaining IATS (Immediate Access Transactions) available for upload or deployment")]
        RemainingIATS,


        /// <summary>
        /// Gets or sets a value indicating whether the test is currently being deployed.
        /// </summary>
        [Description("Indicates whether the test is currently being deployed")]
        TestBeingDeployed,

        /// <summary>
        /// Gets or sets a value indicating whether the item slide download is ready for processing.
        /// </summary>
        [Description("Indicates whether the item slide download is ready for processing")]
        ItemSlideDownloadReady,

        /// <summary>
        /// Specifies an error that occurs when a deployment descriptor does not match the expected configuration.
        /// </summary>
        [Description("Specifies an error that occurs when a deployment descriptor does not match the expected configuration")]
        DeploymentDescriptorMismatch,

        /// <summary>
        /// Gets or sets a value indicating whether the encryption keys have been received.
        /// </summary>
        [Description("Indicates whether the encryption keys have been received")]
        EncryptionKeysReceived,

        /// <summary>
        /// Gets or sets the deployment file manifest that was received.
        /// </summary>
        [Description("Indicates whether the deployment file manifest has been received")]
        DeploymentFileManifestReceived,

        /// <summary>
        /// Gets or sets the manifest data received for the item slide.
        /// </summary>
        [Description("Indicates whether the manifest data for the item slide has been received")]
        ItemSlideManifestReceived,

        /// <summary>
        /// Initiates a handshake request with a remote endpoint to establish communication or verify connectivity.
        /// </summary>
        [Description("Initiates a handshake request with a remote endpoint to establish communication or verify connectivity")]
        RequestHandshake,

        /// <summary>
        /// Aborts the current deployment process, stopping any ongoing operations and rolling back changes if
        /// possible.
        /// </summary>
        /// <remarks>Use this method to halt a deployment that is in progress. Depending on the
        /// deployment system, aborting may leave the environment in a partially updated state. Ensure that aborting
        /// is appropriate for your scenario before calling this method.</remarks>
        [Description("Aborts the current deployment process, stopping any ongoing operations and rolling back changes if possible")]
        AbortDeployment,

        /// <summary>
        /// Indicates that an attempt to abort the deployment has failed.
        /// </summary>
        [Description("Indicates that an attempt to abort the deployment has failed")]
        DeploymentAbortFailed,

        /// <summary>
        /// Indicates that the deployment process has been aborted successfully.
        /// </summary>
        [Description("Indicates that the deployment process has been aborted successfully")]
        DeploymentAborted,

        /// <summary>
        /// Gets a value indicating whether the results are ready for retrieval.
        /// </summary>
        [Description("Indicates whether the results are ready for retrieval")]
        ResultsReady,

        /// <summary>
        /// Represents an error condition where the provided email address does not match the expected value during
        /// verification.
        /// </summary>
        [Description("Represents an error condition where the provided email address does not match the expected value during verification")]
        EmailVerificationMismatch,

        /// <summary>
        /// Represents an entity that is currently in the process of being deployed.
        /// </summary>
        [Description("Represents an entity that is currently in the process of being deployed")]
        IATBeingDeployed,

        /// <summary>
        /// Indicates that the item slides are ready for processing.
        /// </summary>
        [Description("Indicates that the item slides are ready for processing")]
        ItemSlidesReady,

        /// <summary>
        /// Indicates that the request result descriptor is available and can be accessed for further processing or analysis.
        /// </summary>
        [Description("Indicates that the request result descriptor is available")]
        RequestResultDescriptor
    };
}
