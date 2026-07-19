using IronDB.Core;
using System.Runtime.CompilerServices;

namespace IronDB.StorageEngine;

public sealed class PagePositionEqualityComparer : IEqualityComparer<PagePosition>
{
    public static readonly PagePositionEqualityComparer Instance = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PagePosition? x, PagePosition? y)
    {
        if (x == y)
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }

        return x.ScratchPage == y.ScratchPage && x.TransactionId == y.TransactionId && x.JournalNumber == y.JournalNumber && x.IsFreedPageMarker == y.IsFreedPageMarker && x.ScratchNumber == y.ScratchNumber;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(PagePosition obj)
    {
        long v = Hashing.Combine(obj.ScratchPage, obj.TransactionId);
        long w = Hashing.Combine(obj.JournalNumber, obj.ScratchNumber);
        return (int)(v ^ w);
    }
}
