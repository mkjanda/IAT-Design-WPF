using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    internal class RequestFileManifestHandler : IRequestHandler<RequestFileManifestCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public RequestFileManifestHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(RequestFileManifestCommand command, CancellationToken cancellationToken)
        {
            _transactionState.DeploymentId = command.transaction.DeploymentId;
            _transactionState.AuthToken = command.transaction.AuthToken;
            _transactionState.FileManifest.ManifestType = ManifestType.FileManifest;
            await _webSocketService.SendMessage(_transactionState.FileManifest);
            return TransactionResult.Unset;
        }
    }
}
