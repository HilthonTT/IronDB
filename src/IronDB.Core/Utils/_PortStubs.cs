// STUBS: minimal types added to satisfy the compiler while the full port is in progress.
// All members throw NotImplementedException — do not rely on runtime behavior.

namespace IronDB.Core.Utils;

/// <summary>Notifies of memory pressure. Stub.</summary>
public static class LowMemoryNotification
{
    public static void AssertNotAboutToRunOutOfMemory() { /* no-op stub */ }
}

/// <summary>Inspects managed/unmanaged memory usage. Stub.</summary>
public abstract class AbstractLowMemoryMonitor
{
    public static long GetManagedMemoryInBytes() => GC.GetTotalMemory(false);
    public static long GetUnmanagedAllocationsInBytes() => 0;
}

/// <summary>Strongly typed size value. Stub.</summary>
public readonly struct Size
{
    public Size(long value, SizeUnit unit)
    {
        _ = value;
        _ = unit;
    }

    public override string ToString() => throw new NotImplementedException();
}

public enum SizeUnit
{
    Bytes,
    Kilobytes,
    Megabytes,
    Gigabytes,
}


/// <summary>Human-readable size formatting. Stub.</summary>
public static class Sizes
{
    public static string Humane(long bytes) => throw new NotImplementedException();
}

/// <summary>Decimal helper. Stub.</summary>
public sealed class DecimalHelper
{
    public static readonly DecimalHelper Instance = new();

    public bool IsDouble(ref decimal value) => throw new NotImplementedException();
}

/// <summary>Reference equality comparer keyed by object identity. Stub.</summary>
public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
    public static readonly ReferenceEqualityComparer<T> Default = new();

    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

    public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
}

/// <summary>Equality comparer for <c>StringSegment</c> values. Stub.</summary>
public sealed class StringSegmentEqualityStructComparer : IEqualityComparer<StringSegment>
{
    public static readonly StringSegmentEqualityStructComparer BoxedInstance = new();

    public bool Equals(StringSegment x, StringSegment y)
        => throw new NotImplementedException();

    public int GetHashCode(StringSegment obj)
        => throw new NotImplementedException();
}
