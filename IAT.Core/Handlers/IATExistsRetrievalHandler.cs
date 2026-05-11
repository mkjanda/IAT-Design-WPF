using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using MediatR;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the IATExistsRetrievalCommand, which is triggered when checking if an IAT exists. It sends a request for the encryption key to the server.
    /// </summary>
    public class IATExistsRetrievalHandler : IRequestHandler<IATExistsRetrievalCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the IATExistsRetrievalHandler with the necessary dependencies, including the WebSocket service for communication 
        /// and the transaction state to manage the transaction process. It sets up the handler to respond to the IATExistsRetrievalCommand, which is 
        /// expected to be triggered when checking if an IAT exists. The handler will send a request for the encryption key to the server using the WebSocket 
        /// service when this command is received.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage the connection.</param>
        /// <param name="transactionState">The transaction state used to manage the transaction process.</param>
        public IATExistsRetrievalHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }       

        /// <summary>
        /// Handles the retrieval command by sending a request for an encryption key over the WebSocket service.
        /// </summary>
        /// <param name="request">The command containing the parameters required to request the encryption key.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult value
        /// indicating the outcome of the operation.</returns>
        public async Task<TransactionResult> Handle(IATExistsRetrievalCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestEncryptionKey,
                IATName = _transactionState.IATName,
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }
    }
}
