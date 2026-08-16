using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;
using MediatR; 
using System;
using System.Collections.Generic;
using System.Text;


namespace IAT.Core.Handlers
{
    internal class RequestItemSlideManifestHandler : IRequestHandler<RequestItemSlideManifestCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public RequestItemSlideManifestHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(RequestItemSlideManifestCommand command, CancellationToken cancellationToken)
        {
            _transactionState.DeploymentId = command.transaction.DeploymentId;
            _transactionState.AuthToken = command.transaction.AuthToken;
            _transactionState.SlideManifest.ManifestType = ManifestType.ItemSlideManifest;
            await _webSocketService.SendMessage(_transactionState.SlideManifest);
            return TransactionResult.Unset;
        }

    }
}
