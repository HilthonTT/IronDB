using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal readonly struct StringSegmentEqualityStructComparer : IEqualityComparer<StringSegment>
{
    public static IEqualityComparer<StringSegment> BoxedInstance = new StringSegmentEqualityStructComparer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(StringSegment x, StringSegment y)
    {
        return x.AsSpan().SequenceEqual(y.AsSpan());
    }

    public readonly int GetHashCode([DisallowNull] StringSegment obj)
    {
        return (int)Hashing.Marvin32.CalculateInline<Hashing.OrdinalModifier>(obj.AsSpan());
    }
}
