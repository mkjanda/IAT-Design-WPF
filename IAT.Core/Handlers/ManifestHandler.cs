using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// ManifestHandler is responsible for handling the ManifestCommand, which is triggered when a manifest is received from the 
    /// server. It updates the transaction state with the received manifest and sends a message to the server indicating that the 
    /// manifest has been received. The handler returns an unset transaction result, indicating that the transaction is still in 
    /// progress and awaiting further actions or responses.
    /// </summary>
    public class ManifestHandler : IRequestHandler<ManifestCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the ManifestHandler class with the specified WebSocket service and transaction
        /// state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send and receive messages for manifest operations. Cannot be null.</param>
        /// <param name="transactionState">The transaction state object that tracks the current state of transactions. Cannot be null.</param>
        public ManifestHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }   

        /// <summary>
        /// Processes the specified manifest command and updates the transaction state accordingly.
        /// </summary>
        /// <param name="request">The manifest command containing the slide manifest data to be handled.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a TransactionResult indicating the
        /// outcome of the transaction.</returns>
        public async Task<TransactionResult> Handle(ManifestCommand request, CancellationToken cancellationToken)
        {
            _transactionState.SlideManifest = request.manifest;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestItemSlides,
                IATName = _transactionState.IATName
            });
            return TransactionResult.Unset;
        }
    }
}
