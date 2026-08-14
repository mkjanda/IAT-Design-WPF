using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Network;

/// <summary>
/// Retrieves the client's server report (account quota, deployed IATs, administrations remaining)
/// over the persistent WebSocket connection.
/// </summary>
public interface IServerReportService
{
    /// <summary>
    /// Requests a server report for the given product key and email, waits for the response,
    /// and returns the transaction result. On success the report is available on
    /// <see cref="TransactionState.ServerReport"/>.
    /// </summary>
    Task<TransactionResult> RetrieveServerReport(string productKey, string email);
}

/// <summary>
/// Initiates a RequestConnection → RequestServerReport exchange and stores the resulting
/// <see cref="ServerReport"/> on <see cref="TransactionState"/>.
/// Does not tear down the WebSocket — callers own connection lifetime.
/// </summary>
public sealed class ServerReportService : IServerReportService
{
    private static readonly TimeSpan ReportTimeout = TimeSpan.FromSeconds(45);

    private readonly IWebSocketService _webSocketService;
    private readonly TransactionState _transactionState;

    public ServerReportService(IWebSocketService webSocketService, TransactionState transactionState)
    {
        _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
        _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
    }

    /// <inheritdoc />
    public async Task<TransactionResult> RetrieveServerReport(string productKey, string email)
    {
        if (string.IsNullOrWhiteSpace(productKey))
            throw new ArgumentException("Product key is required.", nameof(productKey));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        // Operation-specific handlers must be installed per call — other network services
        // overwrite the same TransactionType keys for their own workflows.
        _webSocketService.TransactionCommands[TransactionType.RequestTransmission] =
            request => new RequestTransmissionServerReportCommand(request);

        _transactionState.Clear();
        _transactionState.Email = email;
        _transactionState.ProductKey = productKey;

        // Ensure the socket is up; do not close it when the report arrives.
        _webSocketService.Start();

        await _webSocketService.SendMessage(new TransactionRequest
        {
            Type = TransactionType.RequestConnection,
            ProductKey = productKey,
            Email = email
        }).ConfigureAwait(false);

        try
        {
            // Preferred async wait — Completion is signaled by TransactionState.SetResult.
            return await _transactionState.Completion
                .WaitAsync(ReportTimeout)
                .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            _transactionState.SetResult(TransactionResult.CannotConnect);
            return TransactionResult.CannotConnect;
        }
    }
}
