using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;
using IAT.Core.Enumerations;
namespace IAT.Core.Handlers
{
    internal class RequestRSAKeyHandler : IRequestHandler<RequestRSAKeyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public RequestRSAKeyHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(RequestRSAKeyCommand request, CancellationToken cancellationToken)
        {
            var key = new EncryptedRSAKey();
            key.Generate(_transactionState.Password);
            await _webSocketService.SendMessage(key);
            _transactionState.Result = TransactionResult.Unset;
            return TransactionResult.Unset;
        }
    }
}
