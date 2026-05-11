using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// PasswordValidResultsHandler is responsible for handling the PasswordValidResultsCommand, which is triggered when the password 
    /// validation process is completed. It sends a request to retrieve the results of the password validation using a WebSocket service 
    /// and returns a success result. This handler plays a crucial role in the authentication flow by ensuring that the results of the 
    /// password validation are properly communicated to the relevant components of the application.
    /// </summary>
    public class PasswordValidResultsHandler : IRequestHandler<PasswordValidResultsCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;

        /// <summary>
        /// Initializes a new instance of the PasswordValidResultsHandler class using the specified WebSocket service.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send or receive messages related to password validation results. Cannot be
        /// null.</param>
        public PasswordValidResultsHandler(IWebSocketService webSocketService)
        {
            _webSocketService = webSocketService;
        }

        /// <summary>
        /// Handles the specified password validation results command by sending a request for transaction results over
        /// the WebSocket service.
        /// </summary>
        /// <param name="request">The command containing the details required to request password validation results.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating the outcome
        /// of the transaction.</returns>
        public async Task<TransactionResult> Handle(PasswordValidResultsCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestResults
            });
            return TransactionResult.Unset;
        }
    }
}
