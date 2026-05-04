using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for processing the RequestTransmissionEMailVerificationCommand, which is responsible for sending an email verification request
    /// </summary>
    public class RequestTransmissionEMailVerificationHandler : IRequestHandler<RequestTransmissionEMailVerificationCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public RequestTransmissionEMailVerificationHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

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