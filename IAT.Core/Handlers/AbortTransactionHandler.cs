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
    /// Handler for the AbortTransactionCommand, which is triggered when a transaction is aborted. It closes the WebSocket connection and shows a notification to the user.  
    /// </summary>
    public class AbortTransactionHandler : IRequestHandler<AbortTransactionCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the AbortTransactionHandler with the necessary dependencies, including the WebSocket service 
        /// for managing the connection and the dialog service for displaying notifications to the user. This setup allows the handler 
        /// to effectively manage transaction abortion scenarios by closing the WebSocket connection and providing user feedback through notifications.
        /// </summary>
        /// <param name="wss">The WebSocket service used to manage the connection.</param>
        /// <param name="dialogService">The dialog service used to display notifications to the user.</param>
        /// <param name="transactionState">The state of the current transaction.</param>
        public AbortTransactionHandler(IWebSocketService wss, IDialogService dialogService, TransactionState transactionState)
        {
            _wss = wss;
            _dialogService = dialogService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles an abort transaction command by closing the active socket connection and notifying the user of the
        /// aborted transaction.
        /// </summary>
        /// <param name="request">The command containing information required to abort the transaction.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>A TransactionResult indicating that the transaction was aborted.</returns>
        public async Task<TransactionResult> Handle(AbortTransactionCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.Aborted.Message, TransactionResult.Aborted.Title);
            _transactionState.Result = TransactionResult.Aborted;
            _transactionState.Event.Set();
            return TransactionResult.Aborted;
        }
    }
}
