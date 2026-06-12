namespace IronDB.Core.Utils;

public sealed class LazyWithExceptionRetry<T>(Func<T> factory)
{
    private Lazy<T> _inner = new Lazy<T>(factory);
    private Func<T>? _factory = factory;
    private bool _faulted;

    public bool IsValueFaulted => _faulted;
    public bool IsValueCreated => _inner.IsValueCreated;

    public T Value
    {
        get
        {
            try
            {
                T value = _inner.Value;
                _faulted = false;
                _factory = null;
                return value;
            }
            catch
            {
                if (_factory is not null)
                {
                    _faulted = true;
                    _inner = new Lazy<T>(_factory);
                }
                throw;
            }
        }
    }
}
