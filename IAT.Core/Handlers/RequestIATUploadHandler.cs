using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Services;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using IAT.Core.Serializable;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    internal class RequestIATUploadHandler : IRequestHandler<RequestIATUploadCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public RequestIATUploadHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(RequestIATUploadCommand request, CancellationToken cancellationToken)
        {
            _transactionState.DeploymentId = request.transaction.LongValues["DeploymentId"];
            _transactionState.UploadTimeMillis = request.transaction.LongValues["DeploymentStartTime"];
            var encKey = new EncryptedRSAKey();
            encKey.Generate(_transactionState.Password);
            _transactionState.RSA = encKey;
            await _webSocketService.SendMessage(encKey);
            return TransactionResult.Unset;
        }
    }
}
