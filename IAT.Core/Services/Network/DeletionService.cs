using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Network;

/// <summary>
/// Deletes a deployed IAT (or only its result data) on the server via the WebSocket.
/// </summary>
public interface IDeletionService
{
    Task<TransactionResult> DeleteTest(string testName, string password);
    Task<TransactionResult> DeleteTestData(string testName, string password);
}

/// <summary>
/// RequestConnection → verify-password → DeleteIAT / DeleteIATData.
/// Uses <see cref="WebSocketTransaction"/> so each attempt starts and ends on a clean socket
/// (avoids hang on a second InvalidPassword against a sticky server session).
/// </summary>
public sealed class DeletionService : IDeletionService
{
    private readonly IWebSocketService _webSocketService;
    private readonly TransactionState _transactionState;
    private readonly ILocalStorageService _localStorage;

    public DeletionService(IWebSocketService webSocketService, TransactionState transactionState, ILocalStorageService localStorage)
    {
        _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
        _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    }

    /// <inheritdoc />
    public async Task<TransactionResult> DeleteTest(string testName, string password)
    {
        if (string.IsNullOrWhiteSpace(testName))
            throw new ArgumentException("Test name is required.", nameof(testName));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        _transactionState.Clear();
        _transactionState.Operation = OperationType.DeleteTest;
        _transactionState.IATName = testName;
        _transactionState.Password = password;
        _transactionState.ProductKey = _localStorage[Field.ProductKey];

        return await WebSocketTransaction.ExecuteAsync(
            _webSocketService,
            _transactionState,
            () => _webSocketService.SendMessage(new TransactionRequest
            {
                Type = TransactionType.RequestConnection,
                ProductKey = _transactionState.ProductKey,
                IATName = _transactionState.IATName
            })).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TransactionResult> DeleteTestData(string testName, string password)
    {
        if (string.IsNullOrWhiteSpace(testName))
            throw new ArgumentException("Test name is required.", nameof(testName));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        _transactionState.Clear();
        _transactionState.Operation = OperationType.DeleteResults;
        _transactionState.IATName = testName;
        _transactionState.Password = password;
        _transactionState.ProductKey = _localStorage[Field.ProductKey];

        return await WebSocketTransaction.ExecuteAsync(
            _webSocketService,
            _transactionState,
            () => _webSocketService.SendMessage(new TransactionRequest
            {
                Type = TransactionType.RequestConnection,
                ProductKey = _transactionState.ProductKey,
                IATName = _transactionState.IATName
            })).ConfigureAwait(false);
    }

}
