using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal unsafe struct UnmanagedSpanComparer : IEqualityComparer<UnmanagedSpan>
{
    public static UnmanagedSpanComparer Instance = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(UnmanagedSpan x, UnmanagedSpan y)
    {
        if (x.Length != y.Length)
        {
            return false;
        }

        if (x.Address == y.Address)
        {
            return true;
        }

        return Memory.CompareInline(x.Address, y.Address, x.Length) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetHashCode(UnmanagedSpan item)
    {
        if (item.Length == 0)
        {
            return 0;
        }

        return (int)Hashing.Marvin32.CalculateInline(item.Address, item.Length);
    }
}