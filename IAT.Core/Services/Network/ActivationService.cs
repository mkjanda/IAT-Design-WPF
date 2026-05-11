using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Network
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

    public class ActivationService : IActivationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the ActivationService with the necessary dependencies, including the WebSocket service 
        /// for communication and the transaction state to manage the activation process. It also sets up the command handler 
        /// for processing activation requests.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used for communication with the server.</param>
        /// <param name="transactionState">The transaction state object used to manage the activation process.</param>
        public ActivationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _transactionState.Clear();
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionActivationCommand(request);
        }

        /// <summary>
        /// Initiates the activation process for a product using the specified product key and user information.
        /// </summary>
        /// <remarks>This method establishes a connection to the activation service and waits for the
        /// activation process to complete before returning the result. The method blocks until the activation response
        /// is received. Ensure that the calling context allows for potential blocking behavior.</remarks>
        /// <param name="productKey">The unique key identifying the product to activate. Cannot be null or empty.</param>
        /// <param name="userName">The name of the user requesting activation. Cannot be null or empty.</param>
        /// <param name="email">The email address associated with the user. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the activation request.</returns>
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
