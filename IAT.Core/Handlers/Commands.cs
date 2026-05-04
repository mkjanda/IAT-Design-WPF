using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Represents a command to indicate that a transaction has completed successfully.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details of the completed transaction. Cannot be null.</param>
    public record TransactionSuccessCommand(TransactionRequest transaction) : IRequest<TransactionResult>;
    
    /// <summary>
    /// Represents a command to mark an email as verified for a specified transaction.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details of the transaction for which the email is being verified. Cannot
    /// be null.</param>
    public record EMailVerifiedCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to indicate that an email address has already been verified for a transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the email verification status. Cannot be null.</param>
    public record EMailAlreadyVerifiedCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to mark a transaction as failed and initiate failure handling logic.
    /// </summary>
    /// <remarks>Use this command to signal that a transaction should be considered unsuccessful. The result
    /// of handling this command typically includes information about the failure and any subsequent actions
    /// taken.</remarks>
    /// <param name="transaction">The transaction request to be marked as failed. Cannot be null.</param>
    public record TransactionFailCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a request to check whether a deployment exists for a given transaction.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details used to identify the deployment.</param>
    public record IATExistsDeploymentCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a request to retrieve the existence status of an Automated Teller transaction.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details required to check the existence of the Automated Teller
    /// transaction. Cannot be null.</param>
    public record IATExistsRetrievalCommand(TransactionRequest transaction) : IRequest<TransactionResult>;
        
    /// <summary>
    /// Represents a request to verify a password as part of a transaction operation.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details required for password verification. Cannot be null.</param>
    public record VerifyPasswordCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command that indicates an invalid password was provided for a transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the invalid password attempt. Cannot be null.</param>
    public record InvalidPasswordCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command indicating that the specified IAT (Implicit Association Test) does not exist.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details of the IAT that does not exist. Cannot be null.</param>
    public record NoSuchIATCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to process a transaction request when the client is in a frozen state.
    /// </summary>
    /// <param name="transaction">The transaction request to be processed. Cannot be null.</param>
    public record ClientFrozenCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to delete a client as part of a transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details required to process the client deletion. Cannot be null.</param>
    public record ClientDeletedCommand(TransactionRequest transaction)  : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request activation of a transaction transmission.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details required to initiate the transmission activation. Cannot be null.</param>
    public record RequestTransmissionActivationCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request email verification for a transaction transmission.
    /// </summary>
    /// <param name="transaction">The transaction request for which email verification is to be initiated. Cannot be null.</param>
    public record RequestTransmissionEMailVerificationCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request that a transmission-related email be resent for a specified transaction.
    /// </summary>
    /// <param name="transaction">The transaction for which the transmission email should be resent. Cannot be null.</param>
    public record RequestTransmissionResendEMailCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to retrieve the results of a transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request for which to retrieve results. Cannot be null.</param>
    public record RequestTransmissionRetrieveResultsCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to retrieve item slides for a specified transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request for which to retrieve item slides. Cannot be null.</param>
    public record RequestTransmissionRetrieveItemSlidesCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to initiate a test deployment of a transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request to be deployed in the test operation. Cannot be null.</param>
    public record RequestTransmissionDeployTestCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to abort an existing transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request to be aborted. Cannot be null.</param>
    public record AbortTransactionCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a request to handle a transaction for a client that does not exist.
    /// </summary>
    /// <param name="transaction">The transaction request to process for the non-existent client. Cannot be null.</param>
    public record NoSuchClientCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command that signals the completion of a transaction request and provides the associated
    /// transaction data.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details of the transaction to be processed. Cannot be null.</param>
    public record ResultsReadyCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to indicate that item slides are ready for processing within a transaction workflow.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the item slides. Cannot be null.</param>
    public record ItemSlidesReadyCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to process a transaction request without performing any activations.
    /// </summary>
    /// <param name="transaction">The transaction request to be processed. Cannot be null.</param>
    public record NoActivationsCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to handle the restoration of a backup for a transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request for which the backup has been restored. Cannot be null.</param>
    public record BackupRestoredCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to handle the failure to restore a backup for a transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request for which the backup could not be restored. Cannot be null.</param>
    public record CannotRestoreBackupCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a request to validate a password as part of a transaction operation.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details required for password validation. Cannot be null.</param>
    public record PasswordValidResultsCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a request to validate a password as part of a transaction workflow.
    /// </summary>
    /// <param name="transaction">The transaction context for which the password validation is performed. Cannot be null.</param>
    public record PasswordValidSlidesCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a request indicating that a password provided for a transaction is invalid.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the invalid password attempt. Cannot be null.</param>
    public record PasswordInvalidCommand(TransactionRequest transaction) : IRequest<TransactionResult>;
}
