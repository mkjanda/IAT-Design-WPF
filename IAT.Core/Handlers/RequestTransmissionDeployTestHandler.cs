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
    /// RequestTransmissionDeployTestHandler is responsible for handling the RequestTransmissionDeployTestCommand, which is triggered when the 
    /// server indicates that the deployment test for the IAT upload should be initiated. Upon receiving this command, the handler sends a 
    /// transaction request to the server to start the IAT upload process, including necessary information such as the IAT name and product key. 
    /// The handler then returns an unset transaction result, indicating that the process is ongoing and awaiting further responses from the server.
    /// </summary>
    public class RequestTransmissionDeployTestHandler : IRequestHandler<RequestTransmissionDeployTestCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the RequestTransmissionDeployTestHandler class with the specified WebSocket
        /// service and transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send and receive messages during the request transmission deploy test. Cannot
        /// be null.</param>
        /// <param name="transactionState">The transaction state that tracks the current status of the request transmission. Cannot be null.</param>
        public RequestTransmissionDeployTestHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the deployment of a test request transmission by sending a transaction request over the WebSocket
        /// service.
        /// </summary>
        /// <param name="request">The command containing the details required to initiate the test request transmission deployment.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the transaction request.</returns>
        public async Task<TransactionResult> Handle(RequestTransmissionDeployTestCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ClientId = request.transaction.ClientID;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestIATUpload,
                IATName = _transactionState.IATName,
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }
    }
}   