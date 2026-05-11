using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for processing the RequestTransmissionActivationCommand, which is responsible for sending an activation request
    /// </summary>
    public class RequestTransmissionActivationHandler : IRequestHandler<RequestTransmissionActivationCommand, TransactionResult>
    {
        readonly private IWebSocketService _webSocketService;
        readonly private TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the RequestTransmissionActivationHandler class with the specified WebSocket
        /// service and transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage WebSocket communications for request transmission.</param>
        /// <param name="transactionState">The transaction state object that tracks the current state of the transaction.</param>
        public RequestTransmissionActivationHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }
        
        /// <summary>
        /// Processes a request to activate a transmission and sends the activation data using a WebSocket service.
        /// </summary>
        /// <param name="request">The command containing the details required to initiate the transmission activation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the activation request.</returns>
        public async Task<TransactionResult> Handle(RequestTransmissionActivationCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new ActivationRequest() 
            {
                FirstName = _transactionState.UserName.Split(' ')[1],
                LastName = _transactionState.UserName.Split(' ')[2],
                EMail = _transactionState.Email,
                ProductCode = _transactionState.ProductKey,
                Title = _transactionState.UserName.Split(' ')[0]
            });
            return TransactionResult.Unset;
        }
    }
}
