using IronDB.Core.Collections;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Utils;

internal sealed class TypeCache<T>(int size)
{
    private readonly FastList<Tuple<Type, T>>[] _buckets = new FastList<Tuple<Type, T>>[size];
    private readonly int _size = size;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(Type type, out T? result)
    {
        Unsafe.SkipInit(out result);

        int typeHash = type.GetHashCode();

        // We get the data and after that we always work from there to avoid harmful race conditions.
        // 
        // The new design emphasizes minimal synchronization overhead.
        // We do direct index lookups and only check a single or small list of items for collisions.
        // This drastically reduces contention compared to a dictionary-based approach.
        FastList<Tuple<Type, T>>? storage = _buckets[typeHash % _size];
        if (storage is null)
        {
            return false;
        }

        ref Tuple<Type, T> item = ref storage.GetAsRef(0);
        if (item.Item1 == type)
        {
            result = item.Item2;
            return true;
        }

        // The idea is that the type cache is big enough so that type collisions are
        // unlikely occurrences. 
        if (storage.Count != 1)
        {
            return TryGetUnlikely(storage, type, out result);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool TryGetUnlikely(FastList<Tuple<Type, T>> storage, Type type, out T result)
    {
        Unsafe.SkipInit(out result);

        // In the uncommon scenario of collisions or multiple inserts,
        // we revert to a simple linear scan. This is rare enough not to impact
        // normal performance, ensuring a good average-case behavior, but since 
        // we have already checked 0 so we will skip it. 
        for (int i = storage.Count - 1; i > 0; i--)
        {
            ref var item = ref storage.GetAsRef(i);
            if (item.Item1 != type)
            {
                continue;
            }

            result = item.Item2;
            return true;
        }

        return false;
    }

    public void Put(Type type, T value)
    {
        int bucket = GetBucket(type);

        // The unsynchronized 'Put' is designed for a high concurrency scenario.
        // It's "okay" if we lose some new entries under race conditions, so long as
        // readers never retrieve the wrong (Type, T) pair. This is a beneficial trade-off
        // for many real-world read-heavy workloads.
        FastList<Tuple<Type, T>> newBucket;
        var storage = _buckets[bucket];

        if (storage is null)
        {
            newBucket = new(4);
        }
        else
        {
            newBucket = new FastList<Tuple<Type, T>>(storage.Count + 1);
            storage.CopyTo(newBucket);
        }

        newBucket.Add(new Tuple<Type, T>(type, value));
        _buckets[bucket] = newBucket;
    }

    private int GetBucket(Type type)
    {
        var hashCode = type.GetHashCode();
        if (hashCode < 0)
        {
            hashCode = -hashCode;
        }

        return hashCode % _size;
    }
}
