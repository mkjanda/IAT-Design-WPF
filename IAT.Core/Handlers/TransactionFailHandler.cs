using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the TransactionFailCommand, which is triggered when a transaction fails. It closes the WebSocket connection and returns a failure result.   
    /// </summary>
    public class TransactionFailHandler : IRequestHandler<TransactionFailCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly TransactionState _state;

        /// <summary>
        /// The constructor initializes the TransactionFailHandler with the necessary dependencies, including the WebSocket service for managing the connection. 
        /// This setup allows the handler to effectively manage transaction failure scenarios by closing the WebSocket connection and returning a failure result 
        /// when a transaction fails.
        /// </summary>
        /// <param name="wss">The WebSocket service used for communication with the server.</param>
        /// <param name="state">The object that contains state information for the transaction.</param>
        public TransactionFailHandler(IWebSocketService wss, TransactionState state)
        {
            _wss = wss;
            _state = state;
        }

        /// <summary>
        /// Handles a transaction failure command by closing the associated WebSocket session and returning a failure
        /// result.
        /// </summary>
        /// <param name="request">The transaction failure command to process. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating that the
        /// transaction has failed.</returns>
        public async Task<TransactionResult> Handle(TransactionFailCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            _state.Result = TransactionResult.Failure;
            _state.Event.Set();
            return TransactionResult.Failure;
        }
    }
}
