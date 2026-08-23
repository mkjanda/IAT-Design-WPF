using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Network
{
    /// <summary>
    /// Defines the interface for the service responsible for activating a product. It includes a method that takes a product key, user name, and email address, and returns a transaction result indicating the success or failure of the activation operation. This service is typically used in scenarios where a user needs to activate their product using a valid product key.
    /// </summary>
    public interface IActivationService
    {
        /// <summary>
        /// Activates a product.
        /// </summary>
        /// <param name="productKey">The product key.</param>
        /// <param name="userName">The name of the user activating the product.</param>
        /// <param name="email">The email of the user activating the product.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the transaction result.</returns>
        Task<TransactionResult> ActivateProduct(string productKey, string userName, string email, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implements the <see cref="IActivationService"/> interface to provide functionality for activating a product. This service sends a request to the server with the provided product key, user name, and email address, and waits for the transaction to complete. It manages the transaction state and communicates with the server via a web socket service. 
    /// </summary>
    public class ActivationService : IActivationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationService"/> class with the specified web socket service and transaction state. It throws an <see cref="ArgumentNullException"/> if either parameter is null.
        /// </summary>
        /// <param name="webSocketService">The web socket service used for communication.</param>
        /// <param name="transactionState">The transaction state.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="webSocketService"/> or <paramref name="transactionState"/> is null.</exception>
        public ActivationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
            _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        }
        /// <summary>
        /// Activates a product by sending a request to the server and waiting for the transaction to complete.
        /// </summary>
        /// <param name="productKey">The product key to activate.</param>
        /// <param name="userName">The name of the user activating the product.</param>
        /// <param name="email">The email of the user activating the product.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The result of the activation transaction.</returns>
        public async Task<TransactionResult> ActivateProduct(string productKey, string userName, string email,
            CancellationToken cancellationToken)
        {
            _transactionState.Clear();
            _transactionState.Operation = OperationType.Activation;
            _transactionState.ProductKey = productKey;
            _transactionState.UserName = userName;
            _transactionState.Email = email;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.RequestConnection,
                ProductKey = productKey
            });
            await _transactionState.Completion.WaitAsync(cancellationToken);
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.ClearSessionState,
                ProductKey = productKey
            });
            return _transactionState.Result;
        }
    }
}
