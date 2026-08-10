using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

using IAT.Core.Services.Network;
using IAT.Core.Serializable;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using javax.naming.@event;


namespace IAT.Core.Handlers
{
    internal class VerifyPasswordHandler : IRequestHandler<VerifyPasswordCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public VerifyPasswordHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.VerifyPassword,
                IATName = _transactionState.IATName,
                TestString = _transactionState.Password
            });
            return TransactionResult.Unset;
        }
    }
}
