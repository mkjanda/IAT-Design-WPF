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
/// RequestConnection → RequestServerReport on a fresh socket per call.
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

        _transactionState.Clear();
        _transactionState.Operation = OperationType.ServerReport;
        _transactionState.Email = email;
        _transactionState.ProductKey = productKey;

        return await WebSocketTransaction.ExecuteAsync(
            _webSocketService,
            _transactionState,
            () => _webSocketService.SendMessage(new TransactionRequest
            {
                Type = TransactionType.RequestConnection,
                ProductKey = productKey,
                Email = email
            }),
            ReportTimeout).ConfigureAwait(false);
    }
}
