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
    /// Handler for processing the RequestTransmissionEMailVerificationCommand, which is responsible for sending an email verification request
    /// </summary>
    public class RequestTransmissionEMailVerificationHandler : IRequestHandler<RequestTransmissionEMailVerificationCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the RequestTransmissionEMailVerificationHandler with the necessary dependencies, including the WebSocket service 
        /// for communication and the transaction state to manage the transaction process. It sets up the handler to respond to the 
        /// RequestTransmissionEMailVerificationCommand, which is expected to be triggered when a request to verify an email is initiated. 
        /// The handler will send a transaction request containing the email and product key information to the server using the WebSocket service 
        /// when this command is received.
        /// </summary>
        /// <param name="webSocketService">The websocket used to send and receive data for email verification operations. Cannot be null.</param>
        /// <param name="transactionState">The transaction state associated with the current operation. Cannot be null.</param>
        public RequestTransmissionEMailVerificationHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Sends a request to initiate an email verification process using the provided command.
        /// </summary>
        /// <param name="request">The command containing the details required to request email verification.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a TransactionResult indicating the
        /// outcome of the request.</returns>
        public async Task<TransactionResult> Handle(RequestTransmissionEMailVerificationCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestEMailVerification,
                StringValues = new Dictionary<string, string> { { "email", _transactionState.Email } },
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }
    }
}   