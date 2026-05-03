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
    /// Handler for the NoSuchClientCommand, which is triggered when the server indicates that there is no such client. 
    /// It shows a notification to the user and closes the WebSocket connection.
    /// </summary>
    internal class NoSuchClientHandler : IRequestHandler<NoSuchClientCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        public NoSuchClientHandler(IWebSocketService webSocketService, IDialogService dialogService)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
        }
        public async Task<TransactionResult> Handle(NoSuchClientCommand request, CancellationToken cancellationToken)
        {
            await _dialogService.ShowNotificationAsync(TransactionResult.NoSuchClient.Message, TransactionResult.NoSuchClient.Title);
            await _webSocketService.CloseSocketAsync();
            return TransactionResult.NoSuchClient;
        }
    }
}
