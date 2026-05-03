using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services; 

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the AbortTransactionCommand, which is triggered when a transaction is aborted. It closes the WebSocket connection and shows a notification to the user.  
    /// </summary>
    internal class AbortTransactionHandler : IRequestHandler<AbortTransactionCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        public AbortTransactionHandler(IWebSocketService wss, IDialogService dialogService)
        {
            _wss = wss;
            _dialogService = dialogService;
        }
        public async Task<TransactionResult> Handle(AbortTransactionCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.Aborted.Message, TransactionResult.Aborted.Title);
            return TransactionResult.Aborted;
        }
    }
}
