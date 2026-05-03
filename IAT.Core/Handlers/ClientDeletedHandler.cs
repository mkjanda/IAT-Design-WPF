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
    /// Handels the client deletion transaction result by closing the WebSocket connection and showing a notification to the user.
    /// </summary>
    internal class ClientDeletedHandler : IRequestHandler<ClientDeletedCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        public ClientDeletedHandler(IWebSocketService wss, IDialogService dialogService)
        {
            _wss = wss;
            _dialogService = dialogService;
        }
        public async Task<TransactionResult> Handle(ClientDeletedCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.ClientDeleted.Message, TransactionResult.ClientDeleted.Title);
            return TransactionResult.ClientDeleted;
        }
    }
}