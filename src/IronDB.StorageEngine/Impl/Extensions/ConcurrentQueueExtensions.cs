using System.Collections.Concurrent;

namespace IronDB.StorageEngine.Impl.Extensions;

public static class ConcurrentQueueExtensions
{
    public static T? Peek<T>(this ConcurrentQueue<T> queue)
        where T : class
    {
        if (!queue.TryPeek(out T? result))
        {
            return null;
        }

        return result;
    }

    // This function does not ensure thread-safty, so the size will not be exact. But it bound the queue without locking. 
    public static ConcurrentQueue<T> Reduce<T>(this ConcurrentQueue<T> queue, int size)
    {
        bool canDequeue = true;
        while (canDequeue && queue.Count > size)
        {
            canDequeue = queue.TryDequeue(result: out T? _);
        }
        return queue;
    }
}
