using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// NoSuchIATHandler is responsible for handling the NoSuchIATCommand, which is triggered when an invalid IAT (Identity Access Token) is encountered.
    /// </summary>
    public class NoSuchIATResultRetrievalHandler : IRequestHandler<NoSuchIATCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _state;

        /// <summary>
        /// Initializes a new instance of the NoSuchIATHandler class with the specified WebSocket and dialog services.
        /// </summary>
        /// <param name="wss">The WebSocket service used to manage WebSocket connections.</param>
        /// <param name="dialogService">The dialog service used to display dialogs or notifications.</param>
        /// <param name="state">The object that contains state information for the transaction.</param>
        public NoSuchIATResultRetrievalHandler(IWebSocketService wss, IDialogService dialogService, TransactionState state)
        {
            _wss = wss;
            _dialogService = dialogService;
            _state = state;
        }

        /// <summary>
        /// Handles a NoSuchIATCommand request by notifying the user and closing the WebSocket connection.
        /// </summary>
        /// <remarks>This method displays a notification to the user and ensures the WebSocket connection
        /// is closed before returning the result.</remarks>
        /// <param name="request">The NoSuchIATCommand request to process. Cannot be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A TransactionResult indicating that the requested IAT command does not exist.</returns>
        public async Task<TransactionResult> Handle(NoSuchIATCommand request, CancellationToken cancellationToken)
        {
            await _dialogService.ShowNotificationAsync(TransactionResult.NoSuchIAT.Message, TransactionResult.NoSuchIAT.Title);
            await _wss.CloseSocketAsync();
            _state.Result = TransactionResult.NoSuchIAT;    
            _state.Event.Set();
            return TransactionResult.NoSuchIAT;
        }
    }
}
