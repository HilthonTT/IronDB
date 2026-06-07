using System.Collections;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Utils;

internal sealed class ReferenceEqualityComparer : IEqualityComparer, IEqualityComparer<object>
{
    public static readonly ReferenceEqualityComparer Default = new();

    public new bool Equals(object? x, object? y)
    {
        return ReferenceEquals(x, y);
    }

    public int GetHashCode(object? obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
}

internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public static readonly ReferenceEqualityComparer<T> Default = new();
    public bool Equals(T? x, T? y)
    {
        return ReferenceEquals(x, y);
    }
    public int GetHashCode(T? obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
}
