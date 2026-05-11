using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handels the client deletion transaction result by closing the WebSocket connection and showing a notification to the user.
    /// </summary>
    public class ClientDeletedHandler : IRequestHandler<ClientDeletedCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _state;

        /// <summary>
        /// The constructor initializes the ClientDeletedHandler with the necessary dependencies, including the WebSocket service for 
        /// managing the connection and the dialog service for displaying notifications to the user. This setup allows the handler to 
        /// effectively manage client deletion scenarios by closing the WebSocket connection and providing user feedback through notifications. 
        /// </summary>
        /// <param name="wss">The WebSocket service used to manage the connection.</param>
        /// <param name="dialogService">The dialog service used to display notifications to the user.</param>
        /// <param name="state">The transaction state used to manage the transaction process.</param>
        public ClientDeletedHandler(IWebSocketService wss, IDialogService dialogService, TransactionState state)
        {
            _wss = wss;
            _dialogService = dialogService;
            _state = state;
        }

        /// <summary>
        /// Handles the client deletion command by closing the WebSocket connection, displaying a notification, and
        /// returning the result of the operation.
        /// </summary>
        /// <param name="request">The command containing information required to process the client deletion.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the client deletion.</returns>
        public async Task<TransactionResult> Handle(ClientDeletedCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.ClientDeleted.Message, TransactionResult.ClientDeleted.Title);
            _state.Result = TransactionResult.ClientDeleted;
            _state.Event.Set();
            return TransactionResult.ClientDeleted;
        }
    }
}