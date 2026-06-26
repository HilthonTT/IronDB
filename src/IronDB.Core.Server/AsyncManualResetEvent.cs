using System.Diagnostics.Contracts;

namespace IronDB.Core.Server;

public sealed class AsyncManualResetEvent : IDisposable
{
    private volatile TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource _cts = new();
    private readonly CancellationToken _token;
    private readonly CancellationTokenRegistration _cancellationTokenRegistration;

    public AsyncManualResetEvent()
    {
        _token = _cts.Token;
    }

    public AsyncManualResetEvent(CancellationToken token)
    {
        _cancellationTokenRegistration = token.Register(
            static (state) => ((TaskCompletionSource<bool>)state!).TrySetCanceled(), _tcs);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _cts.Token;
    }

    public Task<bool> WaitAsync()
    {
        _token.ThrowIfCancellationRequested();
        return _tcs.Task;
    }

    public async Task<bool> WaitAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        // for each wait we will create a new task, since the cancellation token is unique.
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = _tcs.Task.ContinueWith((t) =>
        {
            if (token.IsCancellationRequested)
            {
                tcs.TrySetCanceled();
                return;
            }
            if (t.IsFaulted)
            {
                tcs.TrySetException(t.Exception);
                return;
            }

            if (t.IsCanceled)
            {
                tcs.TrySetCanceled();
                return;
            }
            tcs.TrySetResult(t.Result);
        }, token);

        await using (token.Register(static (state, t) => ((TaskCompletionSource<bool>)state!).TrySetCanceled(t), tcs))
        {
            return await tcs.Task.ConfigureAwait(false);
        }
    }

    public bool IsSet => _tcs.Task.IsCompleted;

    public Task<bool> WaitAsync(TimeSpan timeout)
    {
        return new FrozenAwaiter(_tcs, this).WaitAsync(timeout);
    }

    public FrozenAwaiter GetFrozenAwaiter()
    {
        return new FrozenAwaiter(_tcs, this);
    }

    public readonly struct FrozenAwaiter(TaskCompletionSource<bool> tcs, AsyncManualResetEvent parent)
    {
        private readonly TaskCompletionSource<bool> _tcs = tcs;
        private readonly AsyncManualResetEvent _parent = parent;

        [Pure]
        public Task<bool> WaitAsync()
        {
            return _tcs.Task;
        }

        [Pure]
        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            var waitAsync = _tcs.Task;

            if (_parent._token.IsCancellationRequested)
            {
                return false;
            }

            bool result = await waitAsync.WaitAsync(timeout, _parent._token).ConfigureAwait(false);

            return result;
        }
    }

    public void SetException(Exception e)
    {
        if (_token.IsCancellationRequested)
        {
            _tcs.TrySetCanceled(_token);
            return;
        }
        _tcs.TrySetException(e);
    }

    public void Set()
    {
        if (_token.IsCancellationRequested)
        {
            _tcs.TrySetCanceled(_token);
            return;
        }
        _tcs.TrySetResult(true);
    }

    public void Reset(bool force = false)
    {
        while (true)
        {
            var tcs = _tcs;

            if (_token.IsCancellationRequested)
            {
                tcs.TrySetCanceled(_token);
                return;
            }

            if ((!tcs.Task.IsCompleted && !force) ||
                Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously), tcs) == tcs)
            {
                return;
            }
        }
    }

    public void SetAndResetAtomically()
    {
        while (true)
        {
            var tcs = _tcs;

            if (_token.IsCancellationRequested)
            {
                tcs.TrySetCanceled(_token);
                return;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            if (Interlocked.CompareExchange(ref _tcs, taskCompletionSource, tcs) == tcs)
            {
                tcs.TrySetResult(true);
                break;
            }
        }
    }

    ~AsyncManualResetEvent()
    {
        _cancellationTokenRegistration.Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _cts.Cancel();
        _cancellationTokenRegistration.Dispose();
        _cts.Dispose();
    }
}
