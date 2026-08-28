using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace IAT.Core.Handlers
{
    internal class RequestManifestHandler : IRequestHandler<RequestManifestCommand, TransactionResult>
    {
        private readonly TransactionState _transactionState;
        private readonly IWebSocketService _webSocketService;

        public RequestManifestHandler(TransactionState transactionState, IWebSocketService webSocketService)
        {
            _transactionState = transactionState;
            _webSocketService = webSocketService;
        }

        public async Task<TransactionResult> Handle(RequestManifestCommand request, CancellationToken cancellationToken)
        {
            if (request.transaction.Type == TransactionType.RequestFileManifest)
            {
                _transactionState.FileManifest.ManifestType = ManifestType.FileManifest;
                _transactionState.FileManifest.IATName = _transactionState.IATName;
                _transactionState.FileManifest.ProductKey = _transactionState.ProductKey;
                await _webSocketService.SendMessage(_transactionState.FileManifest);
            } else if (request.transaction.Type == TransactionType.RequestItemSlideManifest) {
                _transactionState.SlideManifest.ManifestType = ManifestType.ItemSlideManifest;
                _transactionState.SlideManifest.IATName = _transactionState.IATName;
                _transactionState.SlideManifest.ProductKey = _transactionState.ProductKey;
                await _webSocketService.SendMessage(_transactionState.SlideManifest);
            }
            return TransactionResult.Unset;
        }
    }
}
