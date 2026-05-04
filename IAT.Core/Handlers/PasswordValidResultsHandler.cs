using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    public class PasswordValidResultsHandler : IRequestHandler<PasswordValidResultsCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        public PasswordValidResultsHandler(IWebSocketService webSocketService)
        {
            _webSocketService = webSocketService;
        }

        public async Task<TransactionResult> Handle(PasswordValidResultsCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestResults
            });
            return TransactionResult.Success;
        }
    }
}
