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
        /// <summary>
        /// Activates a product using the provided product key, user name, and email address. 
        /// This method initiates the activation process by sending a request to the server and 
        /// waits for the result of the activation transaction.
        /// </summary>
        /// <param name="productKey">The product key to activate.</param>
        /// <param name="userName">The user name associated with the activation.</param>
        /// <param name="email">The email address associated with the activation.</param>
        /// <returns>The result of the activation transaction.</returns>
        Task<TransactionResult> ActivateProduct(string productKey, string userName, string email);
    }

    internal class ActivationService : IActivationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public ActivationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionActivationCommand(request);
        }

        public async Task<TransactionResult> ActivateProduct(string productKey, string userName, string email)
        {
            _webSocketService.Start();
            _transactionState.ProductKey = productKey;
            _transactionState.UserName = userName;
            _transactionState.Email = email;
            await _webSocketService.SendMessage(new TransactionRequest() { 
                Transaction = TransactionType.RequestConnection,
                ProductKey = productKey, 
            });
            _transactionState.Event.WaitOne();
            return _transactionState.Result;
        }
    }
}
