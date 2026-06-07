using Nito.AsyncEx;

namespace IronDB.Core.Utils;

internal sealed class DisposeLock(string name)
{
    private readonly string _name = name;
    private readonly AsyncReaderWriterLock _lock = new();
    private readonly CancellationTokenSource _cts = new();

    public IDisposable? EnsureNotDisposed()
    {
        IDisposable? disposable = null;
        try
        {
            disposable = _lock.ReaderLock(_cts.Token);
        }
        catch
        {
            // ignore
        }

        if (disposable is null || _cts.IsCancellationRequested)
        {
            disposable?.Dispose();
            ThrowDisposed();
        }

        return disposable;
    }

    public async ValueTask<IDisposable?> EnsureNotDisposedAsync(bool continueOnCapturedContext = false)
    {
        try
        {
            IDisposable? disposable = await _lock.ReaderLockAsync(_cts.Token).ConfigureAwait(continueOnCapturedContext);
            if (_cts.IsCancellationRequested || disposable is null)
            {
                disposable?.Dispose();
                ThrowDisposed();
            }
            return disposable;
        }
        catch
        {
            ThrowDisposed();
            return null;
        }
    }

    public IDisposable StartDisposing()
    {
        var disposable = _lock.WriterLock(_cts.Token);
        _cts.Cancel();
        return disposable;
    }

    private void ThrowDisposed()
    {
        throw new LockAlreadyDisposedException(_name);
    }
}
