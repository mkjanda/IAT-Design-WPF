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
    /// The RequestTransmissionRetrieveItemSlidesHandler is responsible for handling the RequestTransmissionRetrieveItemSlidesCommand, which is 
    /// triggered when the server indicates that the item slides are ready to be retrieved. Upon receiving this command, the handler sends a 
    /// transaction request to the server to check if the Item Access Token (IAT) exists for the current transaction. The handler then returns 
    /// an unset transaction result, indicating that the process is ongoing and awaiting further responses from the server regarding the 
    /// availability of the item slides.
    /// </summary>
    public class RequestTransmissionRetrieveItemSlidesHandler : IRequestHandler<RequestTransmissionRetrieveItemSlidesCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the RequestTransmissionRetrieveItemSlidesHandler class with the specified
        /// WebSocket service and transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send and receive messages for item slide retrieval operations. Cannot be null.</param>
        /// <param name="transactionState">The transaction state that tracks the current status of the request. Cannot be null.</param>
        public RequestTransmissionRetrieveItemSlidesHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the retrieval of item slides by sending a transaction request over the WebSocket service.
        /// </summary>
        /// <param name="request">The command containing the parameters required to retrieve item slides.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A TransactionResult value indicating the outcome of the transaction. The result is Unset if the operation
        /// does not complete successfully.</returns>
        public async Task<TransactionResult> Handle(RequestTransmissionRetrieveItemSlidesCommand request, CancellationToken cancellationToken)
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
