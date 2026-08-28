using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Handlers;
using IAT.Core.Enumerations;
using IAT.Core.Services.Export;

namespace IAT.Core.Services.Network
{
    public interface ITestDeploymentService
    {
        /// <summary>
        /// Deploys a test package with the specified name, password, and export result over a fresh WebSocket session.
        /// </summary>
        /// <param name="name">The name of the test package.</param>
        /// <param name="password">The password for the test package.</param>
        /// <param name="exportResult">The export result containing the test package data.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The result of the transaction.</returns>
        Task<TransactionResult> Deploy(string name, string password, ExportResult exportResult, 
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Deploys a test package over a fresh WebSocket session per call.
    /// </summary>
    public class TestDeploymentService : ITestDeploymentService
    {
        private readonly IWebSocketService _webSocket;
        private readonly TransactionState _state;
        private readonly ILocalStorageService _localStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDeploymentService"/> class with the specified web socket service, transaction state, and local storage service.
        /// </summary>
        /// <param name="webSocket">The web socket service.</param>
        /// <param name="state">The transaction state.</param>
        /// <param name="localStorage">The local storage service.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
        public TestDeploymentService(IWebSocketService webSocket, TransactionState state, ILocalStorageService localStorage)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        }

        /// <summary>
        /// Deploys a test package with the specified name, password, and export result over a fresh WebSocket session. 
        /// </summary>
        /// <param name="name">The name of the test package.</param>
        /// <param name="password">The password for the test package.</param>
        /// <param name="exportResult">The export result containing the test package data.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The result of the transaction.</returns>
        public async Task<TransactionResult> Deploy(string name, string password, ExportResult exportResult,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(exportResult);

            // Do not Clear() — that would wipe ConfigFile / manifests. Reset completion only.
            _state.ResetCompletion();
            _state.Result = TransactionResult.Unset;
            _state.Operation = OperationType.TestDeployment;
            _state.ConfigFile = exportResult.ConfigFile;
            _state.FileManifest = exportResult.FileManifest;
            _state.SlideManifest = exportResult.SlideManifest;
            _state.ProductKey = _localStorage[Field.ProductKey];
            _state.Password = password;
            _state.IATName = name;
            _state.ProductKey = _localStorage[Field.ProductKey];

            await _webSocket.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.RequestConnection,
                ProductKey = _state.ProductKey
            });
            await _state.Completion.WaitAsync(cancellationToken);
            return _state.Result;
        }
    }
}
