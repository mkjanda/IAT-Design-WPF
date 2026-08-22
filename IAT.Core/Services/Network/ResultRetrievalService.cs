using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using System.Xml.Linq;

namespace IAT.Core.Services.Network
{
    public interface IResultRetrievalService
    {
        /// <summary>
        /// Runs the result-retrieval transaction. On failure returns an empty document —
        /// always inspect <see cref="TransactionState.Result"/>.
        /// </summary>
        Task<XDocument> GetResults(string productKey, string iatName, string password);
    }

    /// <summary>
    /// RequestConnection → RSA / password verify → results download.
    /// Each call uses a fresh WebSocket so a prior InvalidPassword cannot poison the next attempt.
    /// </summary>
    public class ResultRetrievalService : IResultRetrievalService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public ResultRetrievalService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
            _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetResults(string productKey, string iatName, string password)
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

            await WebSocketTransaction.ExecuteAsync(
                _webSocketService,
                _transactionState,
                () => _webSocketService.SendMessage(new TransactionRequest
                {
                    Type = TransactionType.RequestConnection,
                })).ConfigureAwait(false);

            var doc = _transactionState.TestResultsDocument;
            if (doc is null || _transactionState.Result.IsError)
                return new XDocument();

            return doc;
        }
    }
}
