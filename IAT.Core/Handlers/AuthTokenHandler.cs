using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using IAT.Core.Models;
using IAT.Core.Serializable;


namespace IAT.Core.Handlers
{
    internal class AuthTokenHandler : IRequestHandler<AuthTokenCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public AuthTokenHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(AuthTokenCommand request, CancellationToken cancellationToken)
        {
            _transactionState.AuthToken = request.transaction.AuthToken;
            object o = _transactionState.Operation switch
            {
                OperationType.RetrieveResults => new TransactionRequest()
                {
                    Type = TransactionType.RequestResultDescriptor,
                    IATName = _transactionState.IATName
                },
                OperationType.DeleteTest => new TransactionRequest()
                {
                    Type = TransactionType.DeleteIAT,
                    IATName = _transactionState.IATName
                },
                OperationType.DeleteResults => new TransactionRequest()
                {
                    Type = TransactionType.DeleteIATData,
                    IATName = _transactionState.IATName
                }
            };
            await _webSocketService.SendMessage(o);
            return TransactionResult.Unset;
        }
    }
}
