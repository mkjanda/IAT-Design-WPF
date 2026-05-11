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
    /// PasswordValidSlidesHandler is responsible for handling the PasswordValidSlidesCommand, which is triggered when the server confirms that 
    /// the password provided for accessing the slides is valid. Upon receiving this command, the handler sends a request to the server to retrieve 
    /// the item slide manifest, which contains information about the slides available for the transaction. The handler then returns an unset transaction 
    /// result, indicating that the process is ongoing and awaiting further responses from the server.
    /// </summary>
    public class PasswordValidSlidesHandler : IRequestHandler<PasswordValidSlidesCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;

        /// <summary>
        /// Initializes a new instance of the PasswordValidSlidesHandler class with the specified WebSocket service and
        /// transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send and receive messages for slide validation operations. Cannot be null.</param>
        /// <param name="transactionState">The transaction state associated with the current operation. Cannot be null.</param>
        public PasswordValidSlidesHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
        }

        /// <summary>
        /// Handles the specified password validation slides command by sending a transaction request and returning the
        /// transaction result.
        /// </summary>
        /// <param name="request">The command containing the details required to process the password validation slides operation.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the transaction result of the
        /// operation.</returns>
        public async Task<TransactionResult> Handle(PasswordValidSlidesCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestItemSlideManifest
            });
            return TransactionResult.Unset;
        }
    }
}
