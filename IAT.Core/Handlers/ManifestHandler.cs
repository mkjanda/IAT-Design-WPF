using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Handlers
{
    internal class ManifestHandler : IRequestHandler<ManifestCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public ManifestHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }   
        public async Task<TransactionResult> Handle(ManifestCommand request, CancellationToken cancellationToken)
        {
            _transactionState.SlideManifest = request.manifest;
            _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.ItemSlideManifestReceived,
                IATName = _transactionState.IATName
            });
            return TransactionResult.Unset;
        }
    }
}
