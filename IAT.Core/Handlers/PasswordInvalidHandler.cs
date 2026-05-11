using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// PasswordInvalidHandler is responsible for handling the PasswordInvalidCommand, which is triggered when an invalid password 
    /// is detected during a transaction. The handler closes the WebSocket connection to prevent further communication and returns 
    /// a TransactionResult indicating that the password is invalid. This ensures that the transaction is halted and appropriate 
    /// actions can be taken based on the invalid password scenario.
    /// </summary>
    public class PasswordInvalidHandler : IRequestHandler<PasswordInvalidCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _state;

        /// <summary>
        /// Initializes a new instance of the PasswordInvalidHandler class with the specified WebSocket service.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send or receive messages related to password invalidation events. Cannot be
        /// null.</param>
        /// <param name="state">The object that contains state information for the transaction.</param>
        public PasswordInvalidHandler(IWebSocketService webSocketService, TransactionState state)   
        {
            _webSocketService = webSocketService;
            _state = state;
        }

        /// <summary>
        /// Handles a password invalidation command by closing the active WebSocket connection and returning an invalid
        /// password result.
        /// </summary>
        /// <param name="request">The command containing information about the invalid password event to process.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating that the
        /// password was invalid.</returns>
        public async Task<TransactionResult> Handle(PasswordInvalidCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.CloseSocketAsync();
            _state.Result = TransactionResult.InvalidPassword;
            _state.Event.Set();
            return TransactionResult.InvalidPassword;
        }
    }
}