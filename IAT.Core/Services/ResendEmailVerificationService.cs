using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable

using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services
{
    interface IResendEmailVerificationService
    {
        Task<TransactionResult> ResendEmailVerification(string productKey, string email);
    }

    /// <summary>
    /// Reissues verification email for a given product key and email address. This service is used when the user has not received 
    /// the initial verification email or needs to have it sent again for any reason. It initiates the resend process by sending a 
    /// request to the server and waits for the result of the transaction.
    /// </summary>
    internal class ResendEmailVerificationService : IResendEmailVerificationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public ResendEmailVerificationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionResendEMailCommand(request);
        }

        public async Task<TransactionResult> ResendEmailVerification(string productKey, string email)
        {
            _webSocketService.Start();
            _transactionState.Email = email;
            _transactionState.ProductKey = productKey;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestConnection,
                ProductKey = productKey
            });
            _transactionState.Event.WaitOne();
            return _transactionState.Result;
        }
    }
}
