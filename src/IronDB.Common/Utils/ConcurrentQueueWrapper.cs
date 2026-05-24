using System.Collections.Concurrent;

namespace IronDB.Common.Utils;

/// <summary>
/// A thin wrapper around <see cref="ConcurrentQueue{T}"/> that exposes an
/// O(1), eventually-consistent count.
/// </summary>
/// <remarks>
/// The count is maintained with <see cref="Interlocked"/> ops but is not updated
/// atomically with the enqueue/dequeue itself. Callers should treat the count as
/// a hint — it can lag the queue by a moment.
///
/// To prevent count drift this type does NOT inherit from <see cref="ConcurrentQueue{T}"/>;
/// inheriting would let consumers call the base <c>Enqueue</c>/<c>TryDequeue</c> and
/// silently bypass the counter.
/// </remarks>
public sealed class ConcurrentQueueWrapper<T>
{
    private readonly ConcurrentQueue<T> _queue = new();
    private int _queueCount;

    public bool IsEmpty => _queueCount <= 0;

    public int Count
    {
        get
        {
            int curCount = _queueCount;
            return curCount < 0 ? 0 : curCount;
        }
    }

    public bool TryDequeue(out T? result)
    {
        bool dequeued = _queue.TryDequeue(out result);
        if (dequeued)
        {
            Interlocked.Decrement(ref _queueCount);
        }
        return dequeued;
    }

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        Interlocked.Increment(ref _queueCount);
    }
}
