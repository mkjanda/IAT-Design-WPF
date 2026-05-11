using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the ClientFrozenCommand, which is triggered when a client is frozen. It closes the WebSocket connection and shows a notification to the user.
    /// </summary>
    public class ClientFrozenHandler : IRequestHandler<ClientFrozenCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _state;
        /// <summary>
        /// Initializes a new instance of the ClientFrozenHandler class with the specified WebSocket and dialog
        /// services.
        /// </summary>
        /// <param name="wss">The WebSocket service used to communicate with the client.</param>
        /// <param name="dialogService">The dialog service used to display messages or dialogs to the user.</param>
        /// <param name="state">The transaction state used to manage the transaction process.</param>
        public ClientFrozenHandler(IWebSocketService wss, IDialogService dialogService, TransactionState state)
        {
            _wss = wss;
            _dialogService = dialogService;
            _state = state;
        }

        /// <summary>
        /// Handles the client frozen command by closing the WebSocket connection, displaying a notification, and
        /// returning the result indicating the client is frozen.
        /// </summary>
        /// <param name="request">The command containing information required to process the client frozen operation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating the client is
        /// frozen.</returns>
        public async Task<TransactionResult> Handle(ClientFrozenCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.ClientFrozen.Message, TransactionResult.ClientFrozen.Title);
            _state.Result = TransactionResult.ClientFrozen;
            _state.Event.Set();
            return TransactionResult.ClientFrozen;
        }
    }
}
