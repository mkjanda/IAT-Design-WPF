using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;

namespace IAT.Core.Services
{
    public interface IActivationService {
        void ActivateProduct(string productKey, string userName, string email);
    }

    internal class ActivationService : IActivationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>> _transactionHandlers;
        public ActivationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.TransactionSuccess] = (request) => new TransactionSuccessCommand(request);
        }

        public void ActivateProduct(string productKey, string userName, string email)
        {
            _transactionState.ProductKey = productKey;
            _transactionState.UserName = userName;
            _transactionState.Email = email;
            _webSocketService.SendMessage(new ActivationRequest() { 
                EMail = email, 
                Title = userName.Split(" ")[0], 
                FirstName = userName.Split(" ")[1],
                LastName = userName.Split(" ")[2], 
                ProductCode = productKey 
            });
            var source = new CancellationTokenSource();
            var token = source.Token;
            

        }
    }
}
