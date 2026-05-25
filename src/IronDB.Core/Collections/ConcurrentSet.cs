using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace IronDB.Core.Collections;

[DebuggerTypeProxy(typeof(ConcurrentSet<>.DebugProxy))]
public sealed class ConcurrentSet<T> : IEnumerable<T>
    where T : notnull
{
    internal sealed class DebugProxy(ConcurrentSet<T> parent)
    {
        private readonly ConcurrentSet<T> _parent = parent;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items => _parent.Cast<object>().ToArray();
    }

    private int _innerCountForEmpty;
    private readonly ConcurrentDictionary<T, object?> _inner;

    public ConcurrentSet()
    {
        _inner = new ConcurrentDictionary<T, object?>();
    }

    public ConcurrentSet(IEqualityComparer<T> comparer)
    {
        _inner = new ConcurrentDictionary<T, object?>(comparer);
    }

    public int Count => _inner.Count;

    public bool IsEmpty => _innerCountForEmpty == 0;

    public void Add(T item)
    {
        TryAdd(item);
    }

    public bool TryAdd(T item)
    {
        bool b = _inner.TryAdd(item, null);
        if (b)
        {
            Interlocked.Increment(ref _innerCountForEmpty);
        }
        return b;
    }

    public bool Contains(T item)
    {
        return _inner.ContainsKey(item);
    }

    public bool TryRemove(T item)
    {
        var b = _inner.TryRemove(item, out object? _);
        if (b)
        {
            Interlocked.Decrement(ref _innerCountForEmpty);
        }

        return b;
    }

    public void Clear()
    {
        while (true)
        {
            int old = Volatile.Read(ref _innerCountForEmpty);
            _inner.Clear();

            if (Interlocked.CompareExchange(ref _innerCountForEmpty, 0, old) == old)
            {
                break;
            }
        }
    }

    public void UnionWith(IEnumerable<T>? other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));

        foreach (T item in other)
        {
            TryAdd(item);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _inner)
        {
            yield return item.Key;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return Count.ToString("#,#", CultureInfo.InvariantCulture);
    }
}
