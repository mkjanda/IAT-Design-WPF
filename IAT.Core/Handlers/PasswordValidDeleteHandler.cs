using MediatR;
using System;
using System.Collections.Generic;
using IAT.Core.Services.Network;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    internal class PasswordValidDeleteHandler : IRequestHandler<PasswordValidDeleteCommand, TransactionResult>
    {
        private readonly WebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public PasswordValidDeleteHandler(WebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(PasswordValidDeleteCommand request, CancellationToken cancellationToken)
        {
            _transactionState.AuthToken = request.transaction.AuthToken;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.DeleteIAT,
                IATName = _transactionState.IATName
            });
            return TransactionResult.Unset;
        }
    }
}
