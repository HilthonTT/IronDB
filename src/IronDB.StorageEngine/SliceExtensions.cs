using IronDB.Core.Json;
using System.Runtime.CompilerServices;

namespace IronDB.StorageEngine;

public static class SliceExtensions
{
    public static unsafe LazyStringValue GetLazyString(
        this JsonOperationContext context, 
        Slice slice, 
        bool longLived = false)
    {
        return context.GetLazyStringRaw(slice.Content.Ptr, slice.Size, longLived);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartWith(this Slice s1, ReadOnlySpan<byte> s2)
    {
        return s1.AsReadOnlySpan().StartsWith(s2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWith(this Slice s1, ReadOnlySpan<byte> s2)
    {
        return s1.AsReadOnlySpan().EndsWith(s2);
    }

    public static bool Contains(this ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
    {
        int length = first.Length - second.Length;
        if (length < 0)
        {
            return false;
        }

        // This is the last position with enough space to contain the other slice.             
        int end = length;

        byte firstByte = second[0];
        while (end >= 0)
        {
            if (first[end] == firstByte && second.SequenceCompareTo(first.Slice(end, second.Length)) == 0)
            {
                return true;
            }

            end--;
        }

        return false;
    }
}
