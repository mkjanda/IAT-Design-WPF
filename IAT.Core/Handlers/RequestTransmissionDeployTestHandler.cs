using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    internal class RequestTransmissionDeployTestHandler : IRequestHandler<RequestTransmissionDeployTestCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;

        private readonly TransactionState _transactionState;

        public RequestTransmissionDeployTestHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(RequestTransmissionDeployTestCommand request, CancellationToken cancellationToken)
        {
            _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestIATUpload,
                IATName = _transactionState.IATName,
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }
    }
}   