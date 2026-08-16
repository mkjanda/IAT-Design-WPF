using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Serializable;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    internal class RequestConfigFileHandler : IRequestHandler<RequestConfigFileCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public RequestConfigFileHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }
        public async Task<TransactionResult> Handle(RequestConfigFileCommand command, CancellationToken cancellationToken)
        {
            _transactionState.DeploymentId = command.transaction.DeploymentId;
            _transactionState.AuthToken = command.transaction.AuthToken;
            await _webSocketService.SendMessage(_transactionState.ConfigFile);
            return TransactionResult.Unset;
        }
    }
}
