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
    /// Handler for the TransactionSuccessCommand, which is triggered when a transaction is successful. It closes the WebSocket connection and returns a success result.
    /// </summary>
    public class TransactionSuccessHandler : IRequestHandler<TransactionSuccessCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly TransactionState _state;
        /// <summary>
        /// The constructor initializes the TransactionSuccessHandler with the necessary dependencies, including the WebSocket service for managing the connection. This 
        /// setup allows the handler to effectively manage successful transaction scenarios by closing the WebSocket connection and returning a success result when the 
        /// TransactionSuccessCommand is received.
        /// </summary>
        /// <param name="wss">The WebSocket service used for communication with the server.</param>
        /// <param name="state">The object that contains state information for the transaction.</param>
        public TransactionSuccessHandler(IWebSocketService wss, TransactionState state)
        {
            _wss = wss;
            _state = state;
        }   

        /// <summary>
        /// Handles a transaction success command by performing necessary cleanup and returning a successful transaction
        /// result.
        /// </summary>
        /// <param name="request">The transaction success command to process. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a value indicating a successful
        /// transaction.</returns>
        public async Task<TransactionResult> Handle(TransactionSuccessCommand request, CancellationToken cancellationToken)
        {
            await _wss.CloseSocketAsync();
            _state.Result = TransactionResult.Success;
            _state.Event.Set();
            return TransactionResult.Success;
        }
    }
}
