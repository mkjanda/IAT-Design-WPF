using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Handlers;

namespace IAT.Core.Services
{
    interface IEmailVerfiicationService
    {
        Task<TransactionResult> VerifyEmail(string productKey, string email);
    }

    internal class EmailVerfiicationService : IEmailVerfiicationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public TransactionResult Result { get; private set; } = TransactionResult.Unset;
        public EmailVerfiicationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionEMailVerificationCommand(request);
            _webSocketService.TransactionCommands[TransactionType.TransactionSuccess] = (request) => new EMailVerifiedCommand(request);
        }

        public async Task<TransactionResult> VerifyEmail(string productKey, string email)
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