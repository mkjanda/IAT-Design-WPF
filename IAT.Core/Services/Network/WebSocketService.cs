using com.sun.tools.corba.se.idl.constExpr;
using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;
using MediatR;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IAT.Core.Services.Network;

/// <summary>
/// Connection lifecycle states for the persistent WebSocket client.
/// </summary>
public enum WebSocketConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Closing
}

/// <summary>
/// Contract for the application WebSocket client used by activation, deployment,
/// results retrieval, and related transaction workflows.
/// </summary>
public interface IWebSocketService
{
    /// <summary>
    /// Maps server transaction types to MediatR command factories.
    /// Callers may replace individual entries for operation-specific handling.
    /// </summary>
    Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>> TransactionCommands { get; set; }

    /// <summary>Current connection state.</summary>
    WebSocketConnectionState ConnectionState { get; }

    /// <summary>Raised whenever <see cref="ConnectionState"/> changes.</summary>
    event EventHandler<WebSocketConnectionState>? ConnectionStateChanged;

    /// <summary>
    /// Ensures a live connection and starts the receive loop if it is not already running.
    /// Idempotent — safe to call before every transaction.
    /// </summary>
    void Start();

    /// <summary>
    /// Explicitly connects (or reconnects) to the configured endpoint.
    /// Prefer <see cref="Start"/> from transaction services; use this when you need to await readiness.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes <paramref name="message"/> to XML and sends it as a binary WebSocket frame.
    /// Thread-safe; waits until the socket is connected.
    /// </summary>
    Task SendMessage(object message);

    /// <summary>
    /// Gracefully closes the socket, cancels the receive loop, and disables auto-reconnect
    /// until the next <see cref="Start"/> / <see cref="ConnectAsync"/>.
    /// </summary>
    Task CloseSocketAsync();
}

/// <summary>
/// Production-oriented persistent WebSocket client.
/// <list type="bullet">
/// <item>Explicit connect with ws/wss URI normalization</item>
/// <item>Non-empty receive buffer and correct multi-frame reassembly</item>
/// <item>Cancellable receive loop (not fire-and-forget without tracking)</item>
/// <item>Thread-safe send via semaphore</item>
/// <item>ClientWebSocket keep-alive</item>
/// <item>Exponential-backoff auto-reconnect unless intentionally closed</item>
/// <item>Transaction completion signals <see cref="TransactionState"/> without tearing down the connection</item>
/// </list>
/// </summary>
public sealed class WebSocketService : IWebSocketService, IAsyncDisposable
{
    private const int ReceiveBufferSize = 64 * 1024;
    private const int MaxBackoffSeconds = 30;
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(15);

    private readonly IStringResourceService _stringResourceService;
    private readonly IXmlDeserializationService _xmlDeserializationService;
    private readonly TransactionState _transactionState;
    private readonly IDialogService _dialogService;
    private readonly IMediator _mediator;

    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly object _stateGate = new();

    private ClientWebSocket? _socket;
    private CancellationTokenSource? _loopCts;
    private Task? _receiveLoopTask;
    private WebSocketConnectionState _connectionState = WebSocketConnectionState.Disconnected;
    private bool _intentionalClose;
    private int _reconnectAttempt;

    /// <summary>Maps server transaction types to MediatR command factories.</summary>
    public Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>> TransactionCommands { get; set; }

    /// <summary>Current connection state. Thread-safe; raises <see cref="ConnectionStateChanged"/> when changed.</summary>
    public WebSocketConnectionState ConnectionState
    {
        get { lock (_stateGate) return _connectionState; }
        private set
        {
            lock (_stateGate)
            {
                if (_connectionState == value) return;
                _connectionState = value;
            }
            ConnectionStateChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// Raised whenever <see cref="ConnectionState"/> changes. Thread-safe.
    /// </summary>
    public event EventHandler<WebSocketConnectionState>? ConnectionStateChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="WebSocketService"/> with the required dependencies.
    /// </summary>
    /// <param name="stringResourceService">The string resource service.</param>
    /// <param name="xmlDeserializationService">The XML deserialization service.</param>
    /// <param name="transactionState">The transaction state.</param>
    /// <param name="mediator">The mediator.</param>
    /// <param name="dialogService">The dialog service.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public WebSocketService(
        IStringResourceService stringResourceService,
        IXmlDeserializationService xmlDeserializationService,
        TransactionState transactionState,
        IMediator mediator,
        IDialogService dialogService)
    {
        _stringResourceService = stringResourceService ?? throw new ArgumentNullException(nameof(stringResourceService));
        _xmlDeserializationService = xmlDeserializationService ?? throw new ArgumentNullException(nameof(xmlDeserializationService));
        _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        TransactionCommands = new Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>>
        {
            { TransactionType.AbortTransaction, r => new AbortTransactionCommand(r) },
            { TransactionType.EMailAlreadyVerified, r => new EMailAlreadyVerifiedCommand(r) },
            { TransactionType.EncryptionKeyReceived, r => new EncryptionKeyReceivedCommand(r) },
            { TransactionType.IATBeingDeployed, r => new IATBeingDeployedCommand(r) },
            { TransactionType.NoSuchClient, r => new NoSuchClientCommand(r) },
            { TransactionType.RequestIATUpload, r => new RequestIATUploadCommand(r) },
            { TransactionType.TransactionFail, r => new TransactionFailCommand(r) },
            { TransactionType.TransactionSuccess, r => new TransactionSuccessCommand(r) },
            { TransactionType.IATExists, r => new IATExistsCommand(r) },
            { TransactionType.AuthToken, r => new AuthTokenCommand(r) },
            { TransactionType.RequestTransmission, r => new RequestTransmissionCommand(r) }
        };
    }

    /// <summary>
    /// Ensures a live connection and starts the receive loop if it is not already running.
    /// Idempotent — safe to call before every transaction.
    /// </summary>
    public void Start()
    {
        _intentionalClose = false;
        _ = EnsureConnectedAndReceivingAsync();
    }

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _intentionalClose = false;
        await EnsureConnectedAndReceivingAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendMessage(object message)
    {
        ArgumentNullException.ThrowIfNull(message);

        await EnsureConnectedAndReceivingAsync().ConfigureAwait(false);

        var socket = _socket;
        if (socket is null || socket.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not connected.");

        // Serialize as UTF-8 text (what TextWebSocketHandler expects)
        byte[] payload;
        await using (var memStream = new MemoryStream())
        {
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                OmitXmlDeclaration = false,
                Indent = false
            };

            using (var writer = XmlWriter.Create(memStream, settings))
            {
                var ser = new XmlSerializer(message.GetType());
                ser.Serialize(writer, message);
            }

            payload = memStream.ToArray();
        }

        await _sendLock.WaitAsync().ConfigureAwait(false);
        try
        {
            socket = _socket;
            if (socket is null || socket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not connected.");

            using var timeoutCts = new CancellationTokenSource(ConnectTimeout);
            await socket.SendAsync(
                new ArraySegment<byte>(payload),
                WebSocketMessageType.Text,      // ← must be Text
                endOfMessage: true,
                timeoutCts.Token).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }
    

    /// <inheritdoc />
    public async Task CloseSocketAsync()
    {
        _intentionalClose = true;
        ConnectionState = WebSocketConnectionState.Closing;

        var cts = _loopCts;
        _loopCts = null;
        try { cts?.Cancel(); } catch (ObjectDisposedException) { /* ignore */ }

        var loop = _receiveLoopTask;
        _receiveLoopTask = null;

        var socket = _socket;
        _socket = null;

        if (socket is not null)
        {
            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // Best-effort close
            }
            finally
            {
                socket.Dispose();
            }
        }

        if (loop is not null)
        {
            try { await Task.WhenAny(loop, Task.Delay(2000)).ConfigureAwait(false); }
            catch { /* ignore */ }
        }

        cts?.Dispose();
        ConnectionState = WebSocketConnectionState.Disconnected;
        _reconnectAttempt = 0;
    }

    public async ValueTask DisposeAsync()
    {
        await CloseSocketAsync().ConfigureAwait(false);
        _sendLock.Dispose();
        _connectLock.Dispose();
    }

    // ── Internals ──────────────────────────────────────────────────────────

    private async Task EnsureConnectedAndReceivingAsync(CancellationToken externalCt = default)
    {
        await _connectLock.WaitAsync(externalCt).ConfigureAwait(false);
        try
        {
            if (_socket?.State == WebSocketState.Open &&
                _receiveLoopTask is { IsCompleted: false })
            {
                return;
            }

            await ConnectCoreAsync(externalCt).ConfigureAwait(false);
            StartReceiveLoop();
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task ConnectCoreAsync(CancellationToken externalCt)
    {
        ConnectionState = _reconnectAttempt > 0
            ? WebSocketConnectionState.Reconnecting
            : WebSocketConnectionState.Connecting;

        DisposeSocketOnly();

        var uri = ResolveEndpointUri();
        var socket = new ClientWebSocket();
        socket.Options.KeepAliveInterval = KeepAliveInterval;

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        timeoutCts.CancelAfter(ConnectTimeout);

        try
        {
            await socket.ConnectAsync(uri, timeoutCts.Token).ConfigureAwait(false);
        }
        catch
        {
            socket.Dispose();
            ConnectionState = WebSocketConnectionState.Disconnected;
            throw;
        }

        _socket = socket;
        _reconnectAttempt = 0;
        ConnectionState = WebSocketConnectionState.Connected;
    }

    private void StartReceiveLoop()
    {
        if (_receiveLoopTask is { IsCompleted: false })
            return;

        _loopCts?.Dispose();
        _loopCts = new CancellationTokenSource();
        var token = _loopCts.Token;
        _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(token), token);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[ReceiveBufferSize];
        var messageBuffer = new MemoryStream();

        while (!ct.IsCancellationRequested)
        {
            var socket = _socket;
            if (socket is null || socket.State != WebSocketState.Open)
            {
                if (_intentionalClose || ct.IsCancellationRequested)
                    break;

                var recovered = await TryReconnectAsync(ct).ConfigureAwait(false);
                if (!recovered)
                    break;
                continue;
            }

            try
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct)
                    .ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    try
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.None)
                            .ConfigureAwait(false);
                    }
                    catch { /* ignore */ }

                    DisposeSocketOnly();
                    ConnectionState = WebSocketConnectionState.Disconnected;

                    if (_intentionalClose || ct.IsCancellationRequested)
                        break;

                    var recovered = await TryReconnectAsync(ct).ConfigureAwait(false);
                    if (!recovered)
                        break;
                    continue;
                }

                if (result.Count > 0)
                    messageBuffer.Write(buffer, 0, result.Count);

                if (!result.EndOfMessage)
                    continue;

                // Complete message assembled
                messageBuffer.Seek(0, SeekOrigin.Begin);
                try
                {
                    await DispatchMessageAsync(messageBuffer).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WebSocket dispatch error: {ex}");
                    // Do not tear down the connection for a single bad message
                }
                finally
                {
                    messageBuffer.SetLength(0);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebSocket receive error: {ex}");
                DisposeSocketOnly();
                ConnectionState = WebSocketConnectionState.Disconnected;

                if (_intentionalClose || ct.IsCancellationRequested)
                    break;

                var recovered = await TryReconnectAsync(ct).ConfigureAwait(false);
                if (!recovered)
                {
                    try
                    {
                        await _dialogService.ShowNotificationAsync(
                            "Lost connection to the server and could not reconnect. Please try again.",
                            "Connection Error").ConfigureAwait(false);
                    }
                    catch { /* UI may be shutting down */ }
                    break;
                }
            }
        }

        ConnectionState = WebSocketConnectionState.Disconnected;
    }

    private async Task<bool> TryReconnectAsync(CancellationToken ct)
    {
        if (_intentionalClose || ct.IsCancellationRequested)
            return false;

        _reconnectAttempt++;
        var delaySeconds = Math.Min(MaxBackoffSeconds, (int)Math.Pow(2, Math.Min(_reconnectAttempt, 5)));
        // 2, 4, 8, 16, 32→30 capped
        delaySeconds = Math.Min(MaxBackoffSeconds, Math.Max(1, delaySeconds));

        ConnectionState = WebSocketConnectionState.Reconnecting;
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        try
        {
            await _connectLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_intentionalClose) return false;
                await ConnectCoreAsync(ct).ConfigureAwait(false);
                return true;
            }
            finally
            {
                _connectLock.Release();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebSocket reconnect failed (attempt {_reconnectAttempt}): {ex.Message}");
            if (_reconnectAttempt >= 8)
                return false;
            return await TryReconnectAsync(ct).ConfigureAwait(false);
        }
    }

    private async Task DispatchMessageAsync(MemoryStream messageBuffer)
    {
        var message = _xmlDeserializationService.DeserializeUnknownType(messageBuffer);
        if (message is null)
            return;

        IRequest<TransactionResult>? command = message switch
        {
            TransactionRequest tr when TransactionCommands.TryGetValue(tr.Type, out var factory)
                => factory(tr),
            TransactionRequest tr
                => throw new InvalidOperationException($"No handler registered for transaction type '{tr.Type}'."),
            Handshake hs => new HandshakeCommand(hs),
            EncryptedRSAKey key => new RSAKeyCommand(key),
            Manifest manifest => new ManifestCommand(manifest),
            ServerReport serverReport => new ServerReportCommand(serverReport),

            _ => null
        };

        if (command is null)
            return;

        var transactionResult = await _mediator.Send(command).ConfigureAwait(false);

        // Signal waiting callers without tearing down the persistent connection.
        if (transactionResult != TransactionResult.Unset)
        {
            _transactionState.SetResult(transactionResult);
        }
    }

    private Uri ResolveEndpointUri()
    {
        var raw = _stringResourceService.GetString("WebSocketUrl")
                  ?? throw new InvalidOperationException("WebSocketUrl resource is missing.");

        // Accept http(s) configuration and normalize to ws(s).
        if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            raw = "ws://" + raw["http://".Length..];
        else if (raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            raw = "wss://" + raw["https://".Length..];

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "ws" && uri.Scheme != "wss"))
        {
            throw new InvalidOperationException($"WebSocketUrl is not a valid ws/wss URI: '{raw}'");
        }

        return uri;
    }

    private void DisposeSocketOnly()
    {
        var socket = _socket;
        _socket = null;
        if (socket is null) return;
        try { socket.Dispose(); } catch { /* ignore */ }
    }
}
