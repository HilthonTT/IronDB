using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace IronDB.Core.Server.Collections;

// The concept of the small set is about a normal dictionary optimized for accessing recently accessed items. 
// In a sense it behaves like a LRU cache over a dictionary, however, it will not use the backing dictionary unless it has to.
// This version of the Small Set is specially designed to deal with blittable keys in a very efficient manner.
// Since the scanning of the set is based on SIMD instructions (unless they are not available on the platform)
// the cost is linear on the size and may become too high if there are too many novel accesses.
public sealed class SmallSet<TKey, TValue> : IDisposable
    where TKey : unmanaged
{
    private const int Invalid = -1;

    private readonly int _length;
    private readonly TKey[] _keys;
    private readonly TValue[] _values;
    private Dictionary<TKey, TValue>? _overflowStorage;
    private int _currentIdx;

    public SmallSet(int size = 0)
    {
        _length = size > Vector<TKey>.Count ? (size - size % Vector<TKey>.Count) : Vector<TKey>.Count;
        _keys = ArrayPool<TKey>.Shared.Rent(_length);
        _values = ArrayPool<TValue>.Shared.Rent(_length);
        _overflowStorage = null;
        _currentIdx = -1;
    }

    public void Add(TKey key, TValue value)
    {
        int idx = FindKey(key);
        if (idx == Invalid)
        {
            idx = RequestWritableBucket();
        }

        // We have overflowed already. 
        _overflowStorage?[key] = value;

        _keys[idx] = key;
        _values[idx] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FindKey(TKey key)
    {
        int elementIdx = Math.Min(_currentIdx, _length - 1);

        var keys = _keys;
        if (Vector.IsHardwareAccelerated && elementIdx > Vector256<TKey>.Count)
        {
            Vector<TKey> chunk;
            var keyVector = new Vector<TKey>(key);
            while (elementIdx >= Vector256<TKey>.Count)
            {
                // We subtract because we are going to use that even in the case when there are differences.
                elementIdx -= Vector256<TKey>.Count;
                chunk = new Vector<TKey>(keys, elementIdx + 1);
                chunk = Vector.Equals(keyVector, chunk);
                if (chunk == Vector<TKey>.Zero)
                {
                    continue;
                }

                elementIdx = Math.Min(elementIdx + Vector256<TKey>.Count, _length - 1);
                goto Found;
            }

            chunk = new Vector<TKey>(keys);
            chunk = Vector.Equals(keyVector, chunk);
            if (chunk == Vector<TKey>.Zero)
            {
                return Invalid;
            }

            elementIdx = Vector256<TKey>.Count - 1;

        Found:
            elementIdx = Math.Min(elementIdx, _currentIdx);
        }

        while (elementIdx >= 0)
        {
            if (keys[elementIdx].Equals(key))
            {
                return elementIdx;
            }

            elementIdx--;
        }

        return Invalid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int RequestWritableBucket()
    {
        _currentIdx++;

        if (_currentIdx >= _length && _overflowStorage is null)
        {
            var storage = new Dictionary<TKey, TValue>(_currentIdx * 2);
            for (int i = 0; i < _length; i++)
            {
                storage[_keys[i]] = _values[i];
            }
            _overflowStorage = storage;
        }

        return _currentIdx % _length;
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        // PERF: We put this into another method call to shrink the size of TryGetValue in the cases
        // where the inliner would decide to inline the method. Given this method will be rarely executed
        // as if it happens, probably this data structure is not the correct answer; the inliner will 
        // not inline this method ever. 
        [MethodImpl(MethodImplOptions.NoInlining)]
        bool TryGetValueFromOverflowUnlikely(out TValue? value)
        {
            if (_overflowStorage.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }

        int idx = FindKey(key);
        if (idx == Invalid)
        {
            if (_overflowStorage is null)
            {
                Unsafe.SkipInit(out value);
                return false;
            }

            // If we have overflowed, then we will gonna try to find it there. 
            return TryGetValueFromOverflowUnlikely(out value);
        }

        value = _values[idx];
        return true;
    }

    public void Clear()
    {
        Array.Fill(_keys, (TKey)(object)-1);
        Array.Fill(_values, default);
        _overflowStorage?.Clear();
        _currentIdx = -1;
    }

    public void Dispose()
    {
        ArrayPool<TKey>.Shared.Return(_keys);
        ArrayPool<TValue>.Shared.Return(_values);
    }
}
