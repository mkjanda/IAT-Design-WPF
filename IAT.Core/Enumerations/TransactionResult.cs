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
    public enum TransactionResult
    {
        /// <summary>
        /// The transaction result is not set or unknown. This value indicates that the outcome of the transaction has not been determined or is not applicable.
        /// </summary>
        Unset, 
        
        /// <summary>
        /// Represents an error that occurs when a specified client cannot be found.
        /// </summary>
        NoSuchClient, 
        
        /// <summary>
        /// Represents an error condition indicating that a request is invalid or malformed.
        /// </summary>
        InvalidRequest, 

        /// <summary>
        /// Represents an error that occurs when a provided password does not meet the required criteria or is
        /// incorrect.
        /// </summary>
        InvalidPassword,
        
        /// <summary>
        /// Indicates that an operation failed due to a server-side error.
        /// </summary>
        ServerFailure, 
        
        /// <summary>
        /// Gets a value indicating whether there are no activations remaining.
        /// </summary>
        NoActivationsRemaining, 
        
        /// <summary>
        /// Represents an error that occurs when a product code is invalid.
        /// </summary>
        InvalidProductCode, 
        
        /// <summary>
        /// Indicates whether the client is currently frozen and unable to perform operations.
        /// </summary>
        ClientFrozen, 
        
        /// <summary>
        /// Represents an event that occurs when a client is deleted.
        /// </summary>
        ClientDeleted, 
        
        /// <summary>
        /// Indicates that a connection to the target resource could not be established.
        /// </summary>
        CannotConnect, 
        
        /// <summary>
        /// Indicates whether the operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Represents an error condition indicating that the email address has already been verified.  
        /// </summary>
        EmailAlreadyVerified, 
        
        /// <summary>
        /// Represents an error that occurs when the provided email addresses do not match.
        /// </summary>
        EmailMismatch,

        /// <summary>
        /// Represents an error condition where the specified IAT (Import Address Table) does not exist.
        /// </summary>
        NoSuchIAT
    };
}
