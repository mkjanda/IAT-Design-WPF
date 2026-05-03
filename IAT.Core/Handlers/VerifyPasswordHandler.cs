using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the VerifyPasswordCommand, which is triggered when the server requests password verification. 
    /// It decrypts the provided test string using RSA and sends the decrypted string back to the server for verification.
    /// </summary>
    internal class VerifyPasswordHandler : IRequestHandler<VerifyPasswordCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public VerifyPasswordHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }
        public async Task<TransactionResult> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
        {
            var encBytes = Convert.FromBase64String(request.transaction.StringValues["EncryptedTestString"]);
            var rsa = RSA.Create(_transactionState.RSA.GetRSAParameters());
            var decData = rsa.Decrypt(encBytes, RSAEncryptionPadding.Pkcs1);
            _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.VerifyPassword,
                StringValues = new Dictionary<string, string>()
                        {
                            { "DecryptedTestString", Convert.ToBase64String(decData) }
                        },
                IATName = _transactionState.IATName,
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }
    }
}
