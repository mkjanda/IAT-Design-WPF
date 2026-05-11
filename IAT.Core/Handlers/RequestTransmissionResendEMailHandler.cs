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
    /// Responsible for handling the RequestTransmissionResendEMailCommand, which is triggered when the user requests to resend the verification email.
    /// </summary>
    public class RequestTransmissionResendEMailHandler : IRequestHandler<RequestTransmissionResendEMailCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the RequestTransmissionResendEMailHandler class with the specified WebSocket
        /// service and transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send and receive messages related to email transmission requests. Cannot be
        /// null.</param>
        /// <param name="transactionState">The transaction state object that tracks the current state of the email transmission process. Cannot be
        /// null.</param>
        public RequestTransmissionResendEMailHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the command to resend a verification email by initiating a new email verification request.Sends a message to the server requesting 
        /// a new verification email to be sent to the user's email address. The method constructs a TransactionRequest with the appropriate transaction 
        /// type and string values, then sends it through the WebSocket service. The result of the transaction is returned as a TransactionResult, which 
        /// indicates whether the operation was successful or if any errors occurred during the process.
        /// </summary>
        /// <param name="request">The command containing the information required to resend the verification email.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates the outcome of the transaction.</returns>
        public async Task<TransactionResult> Handle(RequestTransmissionResendEMailCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestNewVerificationEMail,
                StringValues = new Dictionary<string, string> { { "email", _transactionState.Email } },
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }
    }
}
