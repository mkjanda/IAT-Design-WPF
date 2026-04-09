using System;
using System.Threading;
using System.Threading.Tasks;

public sealed class Debouncer : IDisposable
{
    private readonly TimeSpan _delay;
    private readonly Action _action;           
    private CancellationTokenSource _cts = new();
    private readonly object _lock = new();

    /// <summary>
    /// Constructs a Debouncer that will execute the provided action after the specified delay, but only if Refresh is not called again within that delay period. This is useful for scenarios 
    /// like UI events where you want to wait for a pause in activity before performing an action (e.g., search-as-you-type). The action will be executed on the captured synchronization 
    /// context, making it safe for UI updates. Dispose should be called when the Debouncer is no longer needed to clean up resources.
    /// </summary>
    /// <param name="delay">The delay period after which the action will be executed if Refresh is not called again.</param>
    /// <param name="action">The action to be executed after the delay period.</param>
    /// <exception cref="ArgumentNullException">Thrown if the action is null.</exception>
    public Debouncer(TimeSpan delay, Action action)
    {
        _delay = delay;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Called to cancel the delay currently being waited on and start a new delay. If this method is called multiple times in quick succession, the action will only be executed
    /// </summary>
    public void Refresh()
    {
        lock (_lock)
        {
            _cts?.Cancel();                    // kill previous pending task
            _cts?.Dispose();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _ = Task.Delay(_delay, token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    _action();                 // fire the real work on the captured context
                }, TaskScheduler.FromCurrentSynchronizationContext()); // keeps it UI-thread safe
        }
    }

    /// <summary>
    /// Disposes of resources used by the Debouncer. This should be called when the Debouncer is no 
    /// longer needed to ensure that any pending tasks are cancelled and resources are released. After calling Dispose,
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}