using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Represents a command to request the transmission of a transaction.
    /// </summary>
    /// <param name="transaction">The transaction request containing the details required to initiate the transmission. Cannot be null.</param>
    public record RequestTransmissionCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to indicate that no such IAT (Implicit Association Test) exists for a given transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the non-existent IAT. Cannot be null.</param>
    public record NoSuchIATCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to check if an IAT (Implicit Association Test) exists for a given transaction request.
    /// </summary>
    /// <param name="transaction">The transaction request to check for the existence of an IAT. Cannot be null.</param>
    public record IATExistsCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

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
    /// Represents a request to handle a transaction for a client that does not exist.
    /// </summary>
    /// <param name="transaction">The transaction request to process for the non-existent client. Cannot be null.</param>
    public record NoSuchClientCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to handle the processing of a server report received as part of a transaction request.
    /// </summary>
    /// <param name="report">The server report to be processed. Cannot be null.</param>
    public record ServerReportCommand(ServerReport report) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request the manifest of files associated with a transaction, indicating that the file manifest is needed for further processing.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the file manifest request. Cannot be null.</param>
    public record RequestManifestCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request the upload of files associated with a transaction, indicating that the file upload is needed for further processing.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the file upload request. Cannot be null.</param>
    public record RequestUploadCommand(TransactionRequest transaction) : IRequest<TransactionResult>;


    /// <summary>
    /// Represents a command to request the upload of an IAT (Implicit Association Test) associated with a transaction, indicating that the IAT upload is needed for further processing.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the IAT upload request. Cannot be null.</param>
    public record RequestIATUploadCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request item slides associated with a transaction, indicating that the item slides are needed for further processing.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the item slides request. Cannot be null.</param>
    public record RequestItemSlidesCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request the configuration file associated with a transaction, indicating that the configuration file is needed for further processing.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the configuration file request. Cannot be null.</param>
    public record RequestConfigFileCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request an encrypted RSA key for a transaction, indicating that the encrypted RSA key is needed for further processing.
    /// </summary>
    /// <param name="transaction">The transaction request associated with the encrypted RSA key request. Cannot be null.</param>
    public record RequestEncryptedRSAKey(TransactionRequest transaction) : IRequest<TransactionResult>;

    /// <summary>
    /// Represents a command to request an authentication token for a transaction, indicating that the authentication token is needed for further processing.
    /// </summary>
    /// <param name="transaction"></param>
    public record AuthTokenCommand(TransactionRequest transaction) : IRequest<TransactionResult>;

    public record RequestEncryptionKeyCommand(TransactionRequest transaction) : IRequest<TransactionResult>;
}