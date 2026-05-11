using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the VerifyPasswordCommand, which is triggered when the server requests password verification. 
    /// It decrypts the provided test string using RSA and sends the decrypted string back to the server for verification.
    /// </summary>
    public class VerifyPasswordHandler : IRequestHandler<VerifyPasswordCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyPasswordHandler"/> class.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used for communication with the server.</param>
        /// <param name="transactionState">The transaction state that tracks the current status and data of the ongoing transaction.</param>
        public VerifyPasswordHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Processes a password verification command by decrypting the provided test string and sending a verification
        /// request over the WebSocket service.
        /// </summary>
        /// <remarks>The method decrypts the test string using RSA and sends the decrypted value for
        /// verification. The returned TransactionResult is always Unset; callers should not expect a definitive
        /// verification result from this method.</remarks>
        /// <param name="request">The command containing the encrypted test string and transaction details to be verified.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the verification process.</returns>
        public async Task<TransactionResult> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
        {
            var encBytes = Convert.FromBase64String(request.transaction.StringValues["EncryptedTestString"]);
            var rsa = RSA.Create(_transactionState.RSA.GetRSAParameters());
            var decData = rsa.Decrypt(encBytes, RSAEncryptionPadding.Pkcs1);
            await _webSocketService.SendMessage(new TransactionRequest()
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
