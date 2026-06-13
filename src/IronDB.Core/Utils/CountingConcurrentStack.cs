using System.Collections.Concurrent;

namespace IronDB.Core.Utils;

internal sealed class CountingConcurrentStack<TItem>
{
    private readonly ConcurrentStack<TItem> _stack = new();
    private long _count;

    public bool IsEmpty => _stack.IsEmpty;

    public long Count => Interlocked.Read(ref _count);

    public bool TryPop(out TItem? item)
    {
        if (!_stack.TryPop(out item))
        {
            item = default;
            return false;
        }

        Interlocked.Decrement(ref _count);
        return true;
    }

    public void Push(TItem item)
    {
        _stack.Push(item);
        Interlocked.Increment(ref _count);
    }

    public IEnumerator<TItem> GetEnumerator()
    {
        return _stack.GetEnumerator();
    }
}
