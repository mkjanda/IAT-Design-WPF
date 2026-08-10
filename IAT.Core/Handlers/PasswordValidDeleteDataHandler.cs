using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Handlers
{
    internal class PasswordValidDeleteDataHandler : IRequestHandler<PasswordValidDeleteDataCommand, TransactionResult>
    {
        private readonly WebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public PasswordValidDeleteDataHandler(WebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(PasswordValidDeleteDataCommand request, CancellationToken cancellationToken)
        {
            _transactionState.AuthToken = request.transaction.AuthToken;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.DeleteIATData,
                IATName = _transactionState.IATName
            });
            return TransactionResult.Unset;
        }
    }

}

