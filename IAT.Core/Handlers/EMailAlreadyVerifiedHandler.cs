using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the EMailAlreadyVerifiedCommand, which is triggered when the server indicates that the email 
    /// has already been verified. It closes the WebSocket connection and returns a success result.
    /// </summary>
    public class EMailAlreadyVerifiedHandler : IRequestHandler<EMailAlreadyVerifiedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the EMailAlreadyVerifiedHandler with the necessary dependencies, including 
        /// the WebSocket service for communication and the transaction state to manage the transaction process. It 
        /// sets up the handler to respond to the EMailAlreadyVerifiedCommand, which is expected to be triggered when 
        /// the server indicates that the email has already been verified. The handler will close the WebSocket connection 
        /// and update the transaction state accordingly when this command is received.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage the connection.</param>
        /// <param name="transactionState">The transaction state used to manage the transaction process.</param>
        public EMailAlreadyVerifiedHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the command indicating that an email has already been verified and performs the necessary
        /// transaction state updates.
        /// </summary>
        /// <param name="request">The command containing information about the already verified email and the associated transaction. Cannot
        /// be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the operation.</returns>
        public async Task<TransactionResult> Handle(EMailAlreadyVerifiedCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ActivationKey = request.transaction.ActivationKey;
            await _webSocketService.CloseSocketAsync();
            _transactionState.Result = TransactionResult.Success;
            _transactionState.Event.Set();
            return TransactionResult.Success;
        }
    }
}