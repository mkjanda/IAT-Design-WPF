using IAT.Core.Enumerations;
using IAT.Core.Models;

namespace IAT.Core.Services.Network;

/// <summary>
/// Shared lifecycle for one-shot network transactions over the WebSocket.
/// <para>
/// Each call tears down any existing connection (clearing sticky server-side session state
/// left by a failed password / half-finished handshake), opens a fresh socket, runs the
/// send phase, waits for <see cref="TransactionState.Completion"/> with a timeout, then
/// always closes the socket again so the next operation starts clean.
/// </para>
/// </summary>
internal static class WebSocketTransaction
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Runs a single transaction with a clean connection before and after.
    /// Caller must have already called <see cref="TransactionState.Clear"/> (or
    /// <see cref="TransactionState.ResetCompletion"/>) and bound operation-specific handlers.
    /// </summary>
    /// <param name="webSocket">WebSocket client.</param>
    /// <param name="state">Shared transaction state whose <see cref="TransactionState.Completion"/> will be awaited.</param>
    /// <param name="sendPhase">Async delegate that sends the initial request(s).</param>
    /// <param name="timeout">Optional override for the completion wait (default 60s).</param>
    public static async Task<TransactionResult> ExecuteAsync(
        IWebSocketService webSocket,
        TransactionState state,
        Func<Task> sendPhase,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(webSocket);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(sendPhase);

        var limit = timeout ?? DefaultTimeout;

        try
        {
            // Drop sticky server session from a prior InvalidPassword / abandoned handshake.
            try
            {
                await webSocket.CloseSocketAsync().ConfigureAwait(false);
            }
            catch
            {
                // Best-effort — socket may already be down.
            }

            await webSocket.ConnectAsync().ConfigureAwait(false);
            await sendPhase().ConfigureAwait(false);

            try
            {
                return await state.Completion.WaitAsync(limit).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // Unblock any waiter; TrySetResult is a no-op if a late handler already completed.
                state.SetResult(TransactionResult.CannotConnect);
                return TransactionResult.CannotConnect;
            }
        }
        finally
        {
            try
            {
                await webSocket.CloseSocketAsync().ConfigureAwait(false);
            }
            catch
            {
                // Best-effort teardown.
            }
        }
    }
}
