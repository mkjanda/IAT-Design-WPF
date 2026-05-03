using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using IAT.Core.Models;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    internal class PasswordValidSlidesHandler : IRequestHandler<PasswordValidSlidesCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;

        public PasswordValidSlidesHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
        }

        public async Task<TransactionResult> Handle(PasswordValidSlidesCommand request, CancellationToken cancellationToken)
        {
            _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestItemSlideManifest
            });
            return TransactionResult.Unset;
        }
    }
}
