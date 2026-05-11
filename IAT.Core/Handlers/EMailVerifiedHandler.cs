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
    /// The EMailVerifiedHandler class is responsible for handling the EMailVerifiedCommand, which is triggered when the server 
    /// confirms that the email has been successfully verified. Upon receiving this command, the handler updates the transaction 
    /// state with the activation key provided in the command and closes the WebSocket connection. Finally, it returns a success 
    /// result to indicate that the email verification process has been completed successfully.
    /// </summary>
    public class EMailVerifiedHandler : IRequestHandler<EMailVerifiedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the EMailVerifiedHandler class with the specified WebSocket service and
        /// transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send or receive messages related to email verification.</param>
        /// <param name="transactionState">The transaction state object that tracks the current state of the email verification process.</param>
        public EMailVerifiedHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }   

        /// <summary>
        /// Handles the completion of the email verification process and finalizes the transaction state.
        /// </summary>
        /// <param name="request">The command containing the details of the verified email transaction. Cannot be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the transaction.</returns>
        public async Task<TransactionResult> Handle(EMailVerifiedCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ActivationKey = request.transaction.ActivationKey;
            await _webSocketService.CloseSocketAsync();
            _transactionState.Result = TransactionResult.Success;
            _transactionState.Event.Set();
            return TransactionResult.Success;
        }
    }
}
