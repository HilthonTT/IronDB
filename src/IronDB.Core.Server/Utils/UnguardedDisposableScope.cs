namespace IronDB.Core.Server.Utils;

public sealed class UnguardedDisposableScope : IDisposable
{
    private readonly LinkedList<IDisposable> _disposables = new();
    private int _delayedDispose;

    public void EnsureDispose(IDisposable toDispose)
    {
        _disposables.AddFirst(toDispose);
    }

    /// <summary>
    /// Delays the disposal and provides the scope that should be disposed first.
    /// </summary>
    /// <returns>An additional disposable scope.</returns>
    public IDisposable Delay()
    {
        _delayedDispose++;
        return this;
    }

    public void Dispose()
    {
        if (_delayedDispose-- > 0)
        {
            return;
        }

        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
