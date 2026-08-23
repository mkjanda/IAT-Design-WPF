using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using System.Xml.Linq;
using net.sf.saxon.serialize;

namespace IAT.Core.Services.Network
{
    public interface IResultRetrievalService
    {
        /// <summary>
        /// Runs the result-retrieval transaction. On failure returns an empty document —
        /// always inspect <see cref="TransactionState.Result"/>.
        /// </summary>
        Task<TestResults> GetResults(string productKey, string iatName, string password, 
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// RequestConnection → RSA / password verify → results download.
    /// Each call uses a fresh WebSocket so a prior InvalidPassword cannot poison the next attempt.
    /// </summary>
    public class ResultRetrievalService : IResultRetrievalService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultRetrievalService"/> class with the specified web socket service and transaction state.
        /// </summary>
        /// <param name="webSocketService">The web socket service used to manage transaction commands and communication.</param>
        /// <param name="transactionState">The transaction state object used to track and manage the current transaction lifecycle.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="webSocketService"/> or <paramref name="transactionState"/> is null.</exception>
        public ResultRetrievalService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
            _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        }

        /// <inheritdoc />
        public async Task<TestResults> GetResults(string productKey, string iatName, string password,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(productKey))
                throw new ArgumentException("Product key is required.", nameof(productKey));
            if (string.IsNullOrWhiteSpace(iatName))
                throw new ArgumentException("IAT name is required.", nameof(iatName));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            _webSocketService.TransactionCommands[TransactionType.RequestConfigFile] =
                request => new RequestConfigFileCommand(request);

            _transactionState.Clear();
            _transactionState.Operation = OperationType.RetrieveResults;
            _transactionState.ProductKey = productKey;
            _transactionState.IATName = iatName;
            _transactionState.Password = password;

            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.RequestConnection,
                ProductKey = productKey,
                IATName = iatName,
            });
            await _transactionState.Completion.WaitAsync(cancellationToken);
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.ClearSessionState,
                ProductKey = productKey
            });
            return _transactionState.TestResults;
        }
    }
}
