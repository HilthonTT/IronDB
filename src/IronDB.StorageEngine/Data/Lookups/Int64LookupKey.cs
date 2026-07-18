using System.Diagnostics.Contracts;

namespace IronDB.StorageEngine.Data.Lookups;

public struct Int64LookupKey(long value) : ILookupKey
{
    public long Value = value;

    public readonly void Reset()
    {

    }

    public readonly long ToLong()
    {
        return Value;
    }

    public static implicit operator Int64LookupKey(long v)
    {
        return new Int64LookupKey(v);
    }

    public static T FromLong<T>(long l)
    {
        if (typeof(T) != typeof(Int64LookupKey))
        {
            throw new NotSupportedException(typeof(T).FullName);
        }

        return (T)(object)new Int64LookupKey(l);
    }

    public static long MinValue => long.MinValue;

    public readonly void Init<T>(Lookup<T> parent) where T : struct, ILookupKey
    {

    }

    public readonly int CompareTo<T>(Lookup<T> parent, long l) where T : struct, ILookupKey
    {
        return Value.CompareTo(l);
    }

    [Pure]
    public readonly int CompareTo<T>(T l) where T : ILookupKey
    {
        if (typeof(T) != typeof(Int64LookupKey))
        {
            throw new NotSupportedException(typeof(T).FullName);
        }

        var o = (Int64LookupKey)(object)l;
        return Value.CompareTo(o.Value);
    }

    [Pure]
    public bool IsEqual<T>(T k) where T : ILookupKey
    {
        if (typeof(T) != typeof(Int64LookupKey))
        {
            throw new NotSupportedException(typeof(T).FullName);
        }

        var o = (Int64LookupKey)(object)k;
        return Value == o.Value;
    }

    public readonly void OnNewKeyAddition<T>(Lookup<T> parent) where T : struct, ILookupKey
    {

    }

    public readonly void OnKeyRemoval<T>(Lookup<T> parent) where T : struct, ILookupKey
    {
    }

    public readonly string ToString<T>(Lookup<T> parent) where T : struct, ILookupKey
    {
        return ToString();
    }

    public readonly int GetTermRefCount<T>(Lookup<T> parent) where T : struct, ILookupKey
    {
        return 1;
    }

    public readonly T Clone<T>(Lookup<T> parent) where T : struct, ILookupKey
    {
        return (T)(object)new Int64LookupKey(Value);
    }

    public readonly override string ToString()
    {
        return Value.ToString();
    }
}