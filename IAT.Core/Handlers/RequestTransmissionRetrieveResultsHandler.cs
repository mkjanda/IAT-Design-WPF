using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// The RequestTransmissionRetrieveResultsHandler is responsible for handling the RequestTransmissionRetrieveResultsCommand, which is 
    /// triggered when the client needs to retrieve the results of a transmission request. Upon receiving this command, the handler sends a 
    /// transaction request to the server to check if the IAT (Interactive Activation Transaction) exists for the current transaction state. 
    /// The handler then returns an unset transaction result, indicating that the process is ongoing and awaiting further responses from the server.
    /// </summary>
    public class RequestTransmissionRetrieveResultsHandler : IRequestHandler<RequestTransmissionRetrieveResultsCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the RequestTransmissionRetrieveResultsHandler class with the specified
        /// WebSocket service and transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send and receive messages related to request transmission.</param>
        /// <param name="transactionState">The transaction state that tracks the current status and data of the ongoing transaction.</param>
        public RequestTransmissionRetrieveResultsHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the retrieval of transaction results by sending a transaction request over the WebSocket service.
        /// </summary>
        /// <param name="request">The command containing the parameters required to retrieve transaction results.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult value
        /// indicating the outcome of the transaction retrieval.</returns>
        public async Task<TransactionResult> Handle(RequestTransmissionRetrieveResultsCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.IATExists,
                IATName = _transactionState.IATName,
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }
    }
}
