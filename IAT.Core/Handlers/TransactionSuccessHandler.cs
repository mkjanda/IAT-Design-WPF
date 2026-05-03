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
    /// Handler for the TransactionSuccessCommand, which is triggered when a transaction is successful. It closes the WebSocket connection and returns a success result.
    /// </summary>
    internal class TransactionSuccessHandler : IRequestHandler<TransactionSuccessCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;

        public TransactionSuccessHandler(IWebSocketService wss)
        {
            _wss = wss;
        }   

        public async Task<TransactionResult> Handle(TransactionSuccessCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            return TransactionResult.Success;
        }
    }
}
