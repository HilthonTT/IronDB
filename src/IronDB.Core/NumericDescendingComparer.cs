using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal readonly struct NumericDescendingComparer : IComparer<long>, IComparer<int>, IComparer<uint>, IComparer<ulong>, IComparer<float>, IComparer<double>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(long x, long y)
    {
        return Math.Sign(y - x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(int x, int y)
    {
        return y - x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(uint x, uint y)
    {
        if (x == y)
        {
            return 0;
        }

        if (x < y)
        {
            return 1;
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(ulong x, ulong y)
    {
        if (x == y)
        {
            return 0;
        }

        if (x < y)
        {
            return 1;
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(float x, float y)
    {
        if (x == y)
        {
            return 0;
        }

        if (x < y)
        {
            return 1;
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(double x, double y)
    {
        if (x == y)
        {
            return 0;
        }

        if (x < y)
        {
            return 1;
        }

        return -1;
    }
}
