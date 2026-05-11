using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the RSAKeyCommand, which is triggered when the server sends the RSA encryption key. It decrypts the key using the transaction password 
    /// and stores it in the transaction state, then sends a request to verify the password with the server.
    /// </summary>
    public class RSAKeyHandler : IRequestHandler<RSAKeyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the RSAKeyHandler with the necessary dependencies, including the WebSocket service for communication and the transaction 
        /// state to manage the transaction process. It sets up the handler to respond to the RSAKeyCommand, which is expected to be triggered when the server 
        /// sends the RSA encryption key. The handler will decrypt the key using the password stored in the transaction state and update the transaction state with 
        /// the decrypted RSA key. After processing the command, it will send a request to the server to verify the password, which is a crucial step in ensuring 
        /// secure communication for subsequent transactions.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used for communication with the server.</param>
        /// <param name="transactionState">The transaction state that tracks the current status and data of the ongoing transaction.</param>
        public RSAKeyHandler(IWebSocketService webSocketService, TransactionState transactionState) 
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Processes an RSA key command by decrypting the provided key, updating the transaction state, and sending a
        /// password verification request over the WebSocket service.
        /// </summary>
        /// <param name="request">The RSA key command containing the key to decrypt and use for the transaction.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A TransactionResult value indicating the outcome of the operation. Returns TransactionResult.Unset after
        /// sending the request.</returns>
        public async Task<TransactionResult> Handle(RSAKeyCommand request, CancellationToken cancellationToken)
        {
            request.Key.DecryptKey(_transactionState.Password);
            _transactionState.RSA = request.Key;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestPasswordVerification,
                ProductKey = _transactionState.ProductKey,
                IATName = _transactionState.IATName
            });
            return TransactionResult.Unset;
        }
    }
}
