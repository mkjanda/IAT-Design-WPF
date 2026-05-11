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
    /// Handler for the NoActivationsCommand, which is triggered when the server indicates that there are no activations remaining for the transaction.
    /// </summary>
    public class NoActivationsHandler : IRequestHandler<NoActivationsCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _state;

        /// <summary>
        /// The constructor initializes the NoActivationsHandler with the necessary dependencies, including the WebSocket service for managing 
        /// the connection and the dialog service for displaying notifications to the user. This setup allows the handler to effectively manage 
        /// scenarios where there are no activations remaining by closing the WebSocket connection and providing user feedback through notifications.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage the connection. Cannot be null.</param>
        /// <param name="dialogService">The dialog service used to show notifications to the user. Cannot be null.</param>
        /// <param name="state">The object that contains state information for the transaction.</param>
        public NoActivationsHandler(IWebSocketService webSocketService, IDialogService dialogService, TransactionState state)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
            _state = state;
        }

        /// <summary>
        /// Handles the specified command by notifying the user that no activations remain and closing the WebSocket
        /// connection. 
        /// </summary>
        /// <param name="request">The command indicating that no activations are remaining. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A transaction result indicating that no activations remain.</returns>
        public async Task<TransactionResult> Handle(NoActivationsCommand request, CancellationToken cancellationToken)
        {
            await _dialogService.ShowNotificationAsync(TransactionResult.NoActivationsRemaining.Message, TransactionResult.NoActivationsRemaining.Title);
            await _webSocketService.CloseSocketAsync();
            _state.Result = TransactionResult.NoActivationsRemaining;
            _state.Event.Set();
            return TransactionResult.NoActivationsRemaining;
        }
    }
}
