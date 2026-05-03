using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services;

namespace IAT.Core.Handlers
{
    internal class EMailVerifiedHandler : IRequestHandler<EMailVerifiedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public EMailVerifiedHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }   
        public async Task<TransactionResult> Handle(EMailVerifiedCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ActivationKey = request.transaction.ActivationKey;
            await _webSocketService.CloseSocketAsync();
            return TransactionResult.Success;
        }
    }
}
