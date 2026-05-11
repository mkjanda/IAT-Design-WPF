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
    /// InvalidPasswordHandler is responsible for handling the InvalidPasswordCommand, which is triggered when an invalid password 
    /// is detected during a transaction. The handler closes the WebSocket connection and shows a notification to the user indicating 
    /// that the password is invalid. This ensures that the user is informed about the issue and can take appropriate action to resolve it.
    /// </summary>
    public class InvalidPasswordHandler : IRequestHandler<InvalidPasswordCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _state;

        /// <summary>
        /// Initializes a new instance of the InvalidPasswordHandler class with the specified WebSocket and dialog
        /// services.
        /// </summary>
        /// <param name="webSocketService">The service used to manage WebSocket communication for handling invalid password scenarios. Cannot be null.</param>
        /// <param name="dialogService">The service used to display dialogs or notifications to the user. Cannot be null.</param>
        /// <param name="state">The object that contains state information for the transaction.</param>
        public InvalidPasswordHandler(IWebSocketService webSocketService, IDialogService dialogService, TransactionState state)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
            _state = state;
        }

        /// <summary>
        /// Handles an invalid password command by closing the current WebSocket connection, displaying a notification
        /// to the user, and returning an invalid password transaction result.
        /// </summary>
        /// <param name="request">The command containing information about the invalid password event to handle.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a transaction result indicating
        /// an invalid password.</returns>
        public async Task<TransactionResult> Handle(InvalidPasswordCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.InvalidPassword.Message, TransactionResult.InvalidPassword.Title);
            _state.Result = TransactionResult.InvalidPassword;
            _state.Event.Set();
            return TransactionResult.InvalidPassword;
        }
    }
}
