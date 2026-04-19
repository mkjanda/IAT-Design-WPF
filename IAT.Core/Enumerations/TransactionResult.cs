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
        Unset, NoSuchClient, InvalidRequest, ServerFailure, NoActivationsRemaining, InvalidProductCode, ClientFrozen, ClientDeleted, CannotConnect, Success,
        EmailAlreadyVerified, EmailMismatch
    };
}
