using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Services;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the RSAKeyCommand, which is triggered when the server sends the RSA encryption key. It decrypts the key using the transaction password 
    /// and stores it in the transaction state, then sends a request to verify the password with the server.
    /// </summary>
    internal class RSAKeyHandler : IRequestHandler<RSAKeyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public RSAKeyHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(RSAKeyCommand request, CancellationToken cancellationToken)
        {
            request.Key.DecryptKey(_transactionState.Password);
            _transactionState.RSA = request.Key;
            _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestPasswordVerification,
                ProductKey = _transactionState.ProductKey,
                IATName = _transactionState.IATName
            });
            return TransactionResult.Unset;
        }
    }
}
