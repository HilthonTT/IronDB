using System.Collections.Concurrent;
using System.Diagnostics;

namespace IronDB.Core.Server.Collections;

public sealed class AsyncQueue<T>(CancellationToken token = default)
    where T : notnull
{
    private readonly ConcurrentQueue<T> _inner = new();
    private readonly AsyncManualResetEvent _event = new(token);

    public int Count => _inner.Count;

    public bool IsEmpty => _inner.IsEmpty;

    public void AddIfEmpty(T item)
    {
        if (_event.IsSet)
        {
            return;
        }

        Enqueue(item);
    }

    public void Enqueue(T item)
    {
        _inner.Enqueue(item);
        _event.Set();
    }

    public bool TryDequeue(out T? result)
    {
        return _inner.TryDequeue(out result);
    }

    public T[] GetAll()
    {
        return _inner.ToArray();
    }

    public async Task<T> DequeueAsync()
    {
        T? result;
        while (!_inner.TryDequeue(out result))
        {
            await _event.WaitAsync().ConfigureAwait(false);
            _event.Reset();
        }
        return result;
    }


    public async Task<Tuple<bool, T?>> TryDequeueAsync(TimeSpan timeout)
    {
        T? result;
        while (!_inner.TryDequeue(out result))
        {
            if (!await _event.WaitAsync(timeout).ConfigureAwait(false))
            {
                return Tuple.Create(false, default(T));
            }
            _event.Reset();
        }

        return Tuple.Create(true, result)!;
    }

    public async Task<Tuple<bool, TValue?>> TryDequeueOfTypeAsync<TValue>(TimeSpan timeout) 
        where TValue : T
    {
        Stopwatch sp = Stopwatch.StartNew();
        while (true)
        {
            T? result;
            while (!_inner.TryDequeue(out result))
            {
                TimeSpan wait = timeout - sp.Elapsed;
                if (wait < TimeSpan.Zero)
                {
                    wait = TimeSpan.Zero;
                }

                if (await _event.WaitAsync(wait).ConfigureAwait(false) == false)
                {
                    return Tuple.Create(false, default(TValue));
                }
                _event.Reset();
            }

            if (result is not TValue)
            {
                continue;
            }
            return Tuple.Create(true, (TValue)result)!;
        }
    }
}
