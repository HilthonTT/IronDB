namespace IronDB.Core.Server.Utils;

public sealed class DisposableScope : IDisposable
{
    private readonly Stack<IDisposable> _disposables = new();
    private int _delayedDispose;

    public void EnsureDispose(IDisposable toDispose)
    {
        _disposables.Push(toDispose);
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

        List<Exception> errors = [];
        while (_disposables.TryPop(out var disposable))
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }

        if (errors.Count != 0)
        {
            throw new AggregateException(errors);
        }
    }
}
