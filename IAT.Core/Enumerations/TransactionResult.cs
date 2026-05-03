using com.sun.corba.se.impl.protocol.giopmsgheaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Specifies the possible outcomes of a product activation attempt.
    /// </summary>
    /// <remarks>Use this enumeration to determine the result of a product activation operation. The values
    /// indicate whether activation was successful or the reason for failure, such as invalid requests, server errors,
    /// or client-specific issues. Handle each result appropriately to provide feedback or take corrective action in
    /// your application.</remarks>
    public abstract record TransactionResult(bool IsSuccess, bool IsError, string Message= "", string Title= "")
    {
        /// <summary>
        /// The transaction result is not set or unknown. This value indicates that the outcome of the transaction has not been determined or is not applicable.
        /// </summary>
        public static TransactionResult Unset = new _Unset(); 
        
        /// <summary>
        /// Represents an error that occurs when a specified client cannot be found.
        /// </summary>
        public static TransactionResult NoSuchClient = new _NoSuchClient();
        
        /// <summary>
        /// Represents an error condition indicating that a request is invalid or malformed.
        /// </summary>
        public static TransactionResult InvalidRequest = new _InvalidRequest(); 

        /// <summary>
        /// Represents an error that occurs when a provided password does not meet the required criteria or is
        /// incorrect.
        /// </summary>
        public static TransactionResult InvalidPassword = new _InvalidPassword();

        /// <summary>
        /// Indicates that an operation failed due to a server-side error.
        /// </summary>
        public static TransactionResult ServerFailure = new _ServerFailure();

        /// <summary>
        /// Gets a value indicating whether there are no activations remaining.
        /// </summary>
        public static TransactionResult NoActivationsRemaining = new _NoActivationsRemaining();
        
        /// <summary>
        /// Represents an error that occurs when a product code is invalid.
        /// </summary>
        public static TransactionResult InvalidProductCode = new _InvalidProductCode();
        
        /// <summary>
        /// Indicates whether the client is currently frozen and unable to perform operations.
        /// </summary>
        public static TransactionResult ClientFrozen = new _ClientFrozen(); 
        
        /// <summary>
        /// Represents an event that occurs when a client is deleted.
        /// </summary>
        public static TransactionResult ClientDeleted = new _ClientDeleted(); 
        
        /// <summary>
        /// Indicates that a connection to the target resource could not be established.
        /// </summary>
        public static TransactionResult CannotConnect = new _CannotConnect();    
        
        /// <summary>
        /// Indicates whether the operation completed successfully.
        /// </summary>
        public static TransactionResult Success = new _Success();

        /// <summary>
        /// Indicates whether the operation completed successfully.
        /// </summary>
        public static TransactionResult Failure = new _Failure();

        /// <summary>
        /// Represents an error condition indicating that the email address has already been verified.  
        /// </summary>
        public static TransactionResult EmailAlreadyVerified = new _EmailAlreadyVerified();
        
        /// <summary>
        /// Represents an error that occurs when the provided email addresses do not match.
        /// </summary>
        public static TransactionResult EmailMismatch = new _EmailMismatch();

        /// <summary>
        /// Represents an error condition where the specified IAT (Import Address Table) does not exist.
        /// </summary>
        public static TransactionResult NoSuchIAT = new _NoSuchIAT();

        /// <summary>
        /// Indicates that the backup of the existing test was successfully restored after a failed redeployment.
        /// </summary>
        public static TransactionResult BackupRestored = new _BackupRestored();

        /// <summary>
        /// Represents a transaction result indicating that a backup cannot be restored.
        /// </summary>
        public static TransactionResult CannotRestoreBackup = new _CannotRestoreBackup();   


        /// <summary>
        /// Indicates that the transaction was canceled by the user or system before completion.
        /// </summary>
        public static TransactionResult Aborted = new _Aborted();


        private sealed record _Unset() : TransactionResult(false, false, "Transaction result is not set or unknown.", "Unknown Result");
        private sealed record _NoSuchClient() : TransactionResult(false, true, "The specified client could not be found.", "Client Not Found");
        private sealed record _InvalidRequest() : TransactionResult(false, true, "The request is invalid or malformed.", "Invalid Request");
        private sealed record _InvalidPassword() : TransactionResult(false, true, "The provided password is invalid.", "Invalid Password");
        private sealed record _ServerFailure() : TransactionResult(false, true, "A server error occurred during the transaction.", "Server Error");
        private sealed record _NoActivationsRemaining() : TransactionResult(false, true, "No activations remaining for this product.", "Activation Limit Reached");
        private sealed record _InvalidProductCode() : TransactionResult(false, true, "The provided product code is invalid.", "Invalid Product Code");
        private sealed record _ClientFrozen() : TransactionResult(false, true, "The client is currently frozen and cannot perform operations.", "Client Frozen");
        private sealed record _ClientDeleted() : TransactionResult(false, true, "The client has been deleted.", "Client Deleted");
        private sealed record _CannotConnect() : TransactionResult(false, true, "Unable to connect to the target resource.", "Connection Failed");
        private sealed record _Success() : TransactionResult(true, false, "The transaction completed successfully.", "Success");
        private sealed record _Failure() : TransactionResult(false, true, "The transaction failed.", "Failure");
        private sealed record _EmailAlreadyVerified() : TransactionResult(true, false, "The email address has already been verified.", "Email Already Verified");
        private sealed record _EmailMismatch() : TransactionResult(false, true, "The provided email addresses do not match.", "Email Mismatch");
        private sealed record _NoSuchIAT() : TransactionResult(false, true, "The specified IAT does not exist.", "IAT Not Found");
        private sealed record _BackupRestored() : TransactionResult(false, false, "The redeployment of your IAT failed and the backup of the existing test was restored.", "Backup Restored");
        private sealed record _CannotRestoreBackup() : TransactionResult(false, true, "The redeployment of your IAT failed and the backup of the existing test could not be restored.", "Backup Restore Failed");
        private sealed record _Aborted() : TransactionResult(false, true, "The transaction was aborted.", "Transaction Aborted");
    };
}
