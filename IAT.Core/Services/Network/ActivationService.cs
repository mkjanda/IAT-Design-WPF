using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Network
{
    public interface IActivationService
    {
        Task<TransactionResult> ActivateProduct(string productKey, string userName, string email);
    }

    public class ActivationService : IActivationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public ActivationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
            _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        }

        public async Task<TransactionResult> ActivateProduct(string productKey, string userName, string email)
        {
            _transactionState.Clear();
            _transactionState.Operation = OperationType.Activation;
            _transactionState.ProductKey = productKey;
            _transactionState.UserName = userName;
            _transactionState.Email = email;

            return await WebSocketTransaction.ExecuteAsync(
                _webSocketService,
                _transactionState,
                () => _webSocketService.SendMessage(new TransactionRequest
                {
                    Type = TransactionType.RequestConnection,
                    ProductKey = productKey,
                })).ConfigureAwait(false);
        }
    }
}
