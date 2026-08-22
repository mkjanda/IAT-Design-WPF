using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Handlers;
using IAT.Core.Enumerations;
using IAT.Core.Services.Export;

namespace IAT.Core.Services.Network
{
    public interface ITestDeploymentService
    {
        Task<TransactionResult> Deploy(string name, string password, ExportResult exportResult);
    }

    /// <summary>
    /// Deploys a test package over a fresh WebSocket session per call.
    /// </summary>
    public class TestDeploymentService : ITestDeploymentService
    {
        private readonly IWebSocketService _webSocket;
        private readonly TransactionState _state;

        public TestDeploymentService(IWebSocketService webSocket, TransactionState state)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public async Task<TransactionResult> Deploy(string name, string password, ExportResult exportResult)
        {
            ArgumentNullException.ThrowIfNull(exportResult);

            _webSocket.TransactionCommands[TransactionType.IATExists] =
                request => new IATExistsCommand(request);
            _webSocket.TransactionCommands[TransactionType.RequestTransmission] =
                request => new RequestTransmissionCommand(request);
            _webSocket.TransactionCommands[TransactionType.RequestItemSlides] =
                request => new RequestItemSlidesCommand(request);
            _webSocket.TransactionCommands[TransactionType.RequestItemSlideManifest] =
                request => new RequestItemSlideManifestCommand(request);
            _webSocket.TransactionCommands[TransactionType.RequestFiles] =
                request => new RequestFilesCommand(request);
            _webSocket.TransactionCommands[TransactionType.RequestFileManifest] =
                request => new RequestFileManifestCommand(request);
            _webSocket.TransactionCommands[TransactionType.TransactionSuccess] =
                request => new DeploymentSuccessCommand(request);
            _webSocket.TransactionCommands[TransactionType.TransactionFail] =
                request => new DeploymentFailCommand(request);

            // Do not Clear() — that would wipe ConfigFile / manifests. Reset completion only.
            _state.ResetCompletion();
            _state.Result = TransactionResult.Unset;
            _state.Operation = OperationType.TestDeployment;
            _state.ConfigFile = exportResult.ConfigFile;
            _state.FileManifest = exportResult.FileManifest;
            _state.SlideManifest = exportResult.SlideManifest;
            _state.Password = password;
            _state.IATName = name;

            return await WebSocketTransaction.ExecuteAsync(
                _webSocket,
                _state,
                () => _webSocket.SendMessage(new TransactionRequest
                {
                    Type = TransactionType.RequestConnection
                }),
                timeout: TimeSpan.FromMinutes(5)).ConfigureAwait(false);
        }
    }
}
