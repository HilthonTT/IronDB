using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace IronDB.Core;

internal sealed class StringSegmentComparer : IComparer<StringSegment>, IEqualityComparer<StringSegment>
{
    public static StringSegmentComparer Ordinal { get; }
           = new StringSegmentComparer(StringComparison.Ordinal, StringComparer.Ordinal);

    public static StringSegmentComparer OrdinalIgnoreCase { get; }
        = new StringSegmentComparer(StringComparison.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);

    private StringSegmentComparer(StringComparison comparison, StringComparer comparer)
    {
        Comparison = comparison;
        Comparer = comparer;
    }

    public StringComparison Comparison { get; }

    public StringComparer Comparer { get; }

    public int Compare(StringSegment x, StringSegment y)
    {
        return StringSegment.Compare(x, y, Comparison);
    }

    public bool Equals(StringSegment x, StringSegment y)
    {
        return StringSegment.Equals(x, y, Comparison);
    }

    public int GetHashCode([DisallowNull] StringSegment obj)
    {
        if (!obj.HasValue)
        {
            return 0;
        }

        return Comparer.GetHashCode(obj.Value ?? string.Empty);
    }
}
