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
    /// <summary>
    /// Deletes a deployed IAT on the server via the WebSocket.
    /// </summary>
    /// <param name="testName">The name of the test to delete.</param>
    /// <param name="password">The password for the test.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the deletion transaction.</returns>
    Task<TransactionResult> DeleteTest(string testName, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes only the result data of a deployed IAT on the server via the WebSocket.
    /// </summary>
    /// <param name="testName">The name of the test whose data to delete.</param>
    /// <param name="password">The password for the test.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the deletion transaction.</returns>
    Task<TransactionResult> DeleteTestData(string testName, string password, CancellationToken cancellationToken);
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletionService"/> class.
    /// </summary>
    /// <param name="webSocketService"></param>
    /// <param name="transactionState"></param>
    /// <param name="localStorage"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DeletionService(IWebSocketService webSocketService, TransactionState transactionState, ILocalStorageService localStorage)
    {
        _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
        _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    }

    /// <summary>
    /// Deletes a deployed IAT on the server via the WebSocket.
    /// </summary>
    /// <param name="testName">The name of the test to delete.</param>
    /// <param name="password">The password for the test.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the deletion transaction.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<TransactionResult> DeleteTest(string testName, string password, CancellationToken cancellationToken)
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

        await _webSocketService.SendMessage(new TransactionRequest()
        {
            ProductKey = _transactionState.ProductKey,
            IATName = _transactionState.IATName,
        });
        await _transactionState.Completion.WaitAsync(cancellationToken);
        await _webSocketService.SendMessage(new TransactionRequest()
        {
            Type = TransactionType.ClearSessionState,
            ProductKey = _transactionState.ProductKey
        });
        return _transactionState.Result;
    }

    /// <summary>
    /// Deletes only the result data for a deployed IAT on the server via the WebSocket.
    /// </summary>
    /// <param name="testName">The name of the test to delete data for.</param>
    /// <param name="password">The password for the test.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the deletion transaction.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<TransactionResult> DeleteTestData(string testName, string password, CancellationToken cancellationToken)
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

        await _webSocketService.SendMessage(new TransactionRequest() { 
            Type = TransactionType.RequestConnection,
            ProductKey = _transactionState.ProductKey,
            IATName = _transactionState.IATName,
        });
        await _transactionState.Completion.WaitAsync(cancellationToken);
        await _webSocketService.SendMessage(new TransactionRequest()
        {
            Type = TransactionType.ClearSessionState,
            ProductKey = _transactionState.ProductKey
        });
        return _transactionState.Result;
    }
}
