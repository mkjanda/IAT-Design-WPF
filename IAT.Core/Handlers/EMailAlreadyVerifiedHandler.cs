using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the EMailAlreadyVerifiedCommand, which is triggered when the server indicates that the email 
    /// has already been verified. It closes the WebSocket connection and returns a success result.
    /// </summary>
    public class EMailAlreadyVerifiedHandler : IRequestHandler<EMailAlreadyVerifiedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public EMailAlreadyVerifiedHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(EMailAlreadyVerifiedCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ActivationKey = request.transaction.ActivationKey;
            await _webSocketService.CloseSocketAsync();
            return TransactionResult.Success;
        }
    }
}