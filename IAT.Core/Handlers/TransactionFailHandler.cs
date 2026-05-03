using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using IAT.Core.Services;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the TransactionFailCommand, which is triggered when a transaction fails. It closes the WebSocket connection and returns a failure result.   
    /// </summary>
    internal class TransactionFailHandler : IRequestHandler<TransactionFailCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;

        public TransactionFailHandler(IWebSocketService wss)
        {
            _wss = wss;
        }

        public async Task<TransactionResult> Handle(TransactionFailCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            return TransactionResult.Failure;
        }
    }
}
