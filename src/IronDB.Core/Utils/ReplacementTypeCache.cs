using IronDB.Core.Binary;
using IronDB.Core.Collections;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Utils;

/// <summary>
/// A lightweight, hash-based cache for associating .NET <see cref="Type"/> objects with values of a generic type <typeparamref name="T"/>.
/// This cache prioritizes performance over strict synchronization, making it suitable for scenarios where occasional cache misses
/// are acceptable but incorrect (Type, T) mappings must never occur.
/// </summary>
internal sealed class ReplacementTypeCache<T>
{
    // The backing store of Type -> (Type, T) pairs.
    // We do unsynchronized reads/writes, relying on:
    //  1) Atomic reference assignment in .NET
    //  2) The check (item.Item1 == type) to avoid incorrect Type mismatches
    //  3) It's acceptable for TryGet to return false in race conditions
    // This is a simpler, specialized design used for an even lighter collision approach.
    // Instead of a collection of items (like in TypeCache<T>), we store a single entry per slot.
    // Collisions overwrite the previous entry. This drastically reduces overhead in reading
    // at the cost of occasionally losing older entries.

    private const int DefaultSize = 64;

    private FastList<Tuple<Type, T>> _buckets;
    private readonly int _size;
    private readonly int _mask;

    public ReplacementTypeCache(int size = DefaultSize)
    {
        // We use power-of-two sizing (rounded up via Bits.PowerOf2)
        // to make (hash & mask) faster than % operations. This is a micro-optimization
        // that can matter when this call is extremely hot.
        _size = Bits.PowerOf2(size);
        _mask = _size - 1;
        _buckets = new(_size);
    }

    /// <summary>
    /// Attempts to retrieve the cached value for the given Type.
    /// This lookup is thread-safe under races in the sense that
    /// it will never return an incorrect (Type, T) pair. However,
    /// it may return false (i.e., a cache miss) if a concurrent
    /// writer has not yet become visible or if there's a hash collision
    /// for a different Type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(Type type, out T result)
    {
        // The simplified approach uses a single entry per bucket.
        // This yields a very fast read path. If the entry is for a different type
        // or a race overwrote it, we just return false => 'cache miss'.
        var item = _buckets[type.GetHashCode() & _mask];
        if (item is not null && item.Item1 == type)
        {
            result = item.Item2;
            return true;
        }

        // If not matched, return false.
        // We do not attempt to handle for collisions or partial updates here, as the design accepts these as 'misses'.
        Unsafe.SkipInit(out result);
        return false;
    }

    /// <summary>
    /// Puts a (Type, T) pair into the cache at the computed bucket index.
    /// This is unsynchronized and may overwrite an existing entry.
    /// This design means we can lose old entries under collision but 
    /// we never produce a wrong (Type, T) on readers. 
    /// Readers can safely read the tuple reference, but might see it 
    /// late or see an older one => which we accept as a 'miss'.
    /// </summary>
    public void Put(Type type, T value)
    {
        // Collisions simply overwrite. No chain or recheck.
        // This drastically cuts memory overhead and lock contention,
        // making it ideal for a scenario where each type is typically
        // hashed consistently, with few collisions in practice.
        _buckets[type.GetHashCode() & _mask] = new Tuple<Type, T>(type, value);
    }
}
