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
    /// Handler for the ClientFrozenCommand, which is triggered when a client is frozen. It closes the WebSocket connection and shows a notification to the user.
    /// </summary>
    internal class ClientFrozenHandler : IRequestHandler<ClientFrozenCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        public ClientFrozenHandler(IWebSocketService wss, IDialogService dialogService)
        {
            _wss = wss;
            _dialogService = dialogService;
        }
        public async Task<TransactionResult> Handle(ClientFrozenCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.ClientFrozen.Message, TransactionResult.ClientFrozen.Title);
            return TransactionResult.ClientFrozen;
        }
    }
}
