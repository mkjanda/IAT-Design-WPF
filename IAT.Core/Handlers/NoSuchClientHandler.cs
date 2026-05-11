using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the NoSuchClientCommand, which is triggered when the server indicates that there is no such client. 
    /// It shows a notification to the user and closes the WebSocket connection.
    /// </summary>
    public class NoSuchClientHandler : IRequestHandler<NoSuchClientCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the NoSuchClientHandler with the necessary dependencies, including the WebSocket service for 
        /// managing the connection and the dialog service for displaying notifications to the user. This setup allows the handler to 
        /// effectively manage scenarios where the server indicates that there is no such client by showing a notification to the user 
        /// and closing the WebSocket connection.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage the connection. Cannot be null.</param>
        /// <param name="dialogService">The dialog service used to show notifications to the user. Cannot be null.</param>
        /// <param name="transactionState">The object that contains state information for the transaction.</param>
        public NoSuchClientHandler(IWebSocketService webSocketService, IDialogService dialogService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles a command indicating that a client does not exist by notifying the user and closing the WebSocket
        /// connection.
        /// </summary>
        /// <remarks>This method displays a notification to the user and ensures the WebSocket connection
        /// is closed when a non-existent client is detected.</remarks>
        /// <param name="request">The command containing information about the missing client to be handled.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// that the client was not found.</returns>
        public async Task<TransactionResult> Handle(NoSuchClientCommand request, CancellationToken cancellationToken)
        {
            await _dialogService.ShowNotificationAsync(TransactionResult.NoSuchClient.Message, TransactionResult.NoSuchClient.Title);
            await _webSocketService.CloseSocketAsync();
            _transactionState.Result = TransactionResult.NoSuchClient;
            _transactionState.Event.Set();  
            return TransactionResult.NoSuchClient;
        }
    }
}
