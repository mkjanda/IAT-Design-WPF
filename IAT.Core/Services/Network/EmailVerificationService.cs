using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Handlers;
using System.Security.RightsManagement;

namespace IAT.Core.Services.Network
{
    /// <summary>
    /// The interface IEmailVerificationService defines a contract for a service that handles email verification processes. 
    /// It includes a method for verifying an email address associated with a product key, which is typically used in scenarios 
    /// such as account activation or user registration. The service is designed to communicate with a server using WebSocket 
    /// connections and manage the state of the transaction during the verification process. Implementations of this interface 
    /// will provide the logic for sending verification requests and handling responses from the server to determine the result 
    /// of the email verification.
    /// </summary>
    public interface IEmailVerificationService
    {
        /// <summary>
        /// Initiates the email verification process for a specified product and email address.
        /// </summary>
        /// <param name="productKey">The unique key identifying the product for which the email verification is requested. Cannot be null or
        /// empty.</param>
        /// <param name="email">The email address to verify. Must be a valid, non-empty email address.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the verification request.</returns>
        Task<TransactionResult> VerifyEmail(string productKey, string email);

        /// <summary>
        /// The activation key for the software
        /// </summary>
        public string ActivationKey { get; }
    }

    /// <summary>
    /// Provides functionality to verify an email address as part of a transaction process using a WebSocket-based
    /// communication service.
    /// </summary>
    /// <remarks>This service coordinates the email verification workflow by interacting with a WebSocket
    /// service and managing transaction state. It is typically used in scenarios where email verification is required
    /// to complete a product registration or similar transaction. The service is not thread-safe and should not be
    /// shared across concurrent operations.</remarks>
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the EmailVerificationService class with the specified WebSocket service and
        /// transaction state dependencies.
        /// </summary>
        /// <remarks>This constructor configures the WebSocket service to handle email
        /// verification-related transaction commands. The provided dependencies must not be null.</remarks>
        /// <param name="webSocketService">The WebSocket service used to handle transaction commands related to email verification.</param>
        /// <param name="transactionState">The transaction state object that manages the current state of the transaction process.</param>
        public EmailVerificationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionEMailVerificationCommand(request);
            _webSocketService.TransactionCommands[TransactionType.TransactionSuccess] = (request) => new EMailVerifiedCommand(request);
        }

        /// <summary>
        /// Initiates an email verification process for the specified product key and email address.
        /// </summary>
        /// <remarks>This method starts a new transaction to verify the provided email address for the
        /// given product key. The operation is asynchronous and may block until the verification process completes.
        /// Ensure that the method is called from a context that allows for potential blocking behavior.</remarks>
        /// <param name="productKey">The unique identifier for the product to associate with the email verification request. Cannot be null or
        /// empty.</param>
        /// <param name="email">The email address to verify. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the verification process.</returns>
        async public Task<TransactionResult> VerifyEmail(string productKey, string email)
        {
            _webSocketService.Start();
            _transactionState.Email = email;
            _transactionState.ProductKey = productKey;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestConnection,
                ProductKey = productKey
            });
            _transactionState.Event.WaitOne();
            return _transactionState.Result;
        }
        
        /// <summary>
        /// Gets the activation key for the software.
        /// </summary>
        public string ActivationKey => _transactionState.ActivationKey;
    }   
}