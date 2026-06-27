using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Server.Utils.VxSort;

public static unsafe partial class VectorizedSort
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorLog2(uint n)
    {
        return 31 - BitOperations.LeadingZeroCount(n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorLog2PlusOne(uint n)
    {
        return FloorLog2(n) + 1;
    }

    public static void Run<T>([NotNull] T[] array)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));

        if (!AdvInstructionSet.X86.IsSupportedAvx256)
        {
            MemoryExtensions.Sort(array.AsSpan());
            return;
        }

        fixed (T* arrayPtr = array)
        {
            T* left = arrayPtr;
            T* right = arrayPtr + array.Length - 1;
            Run(left, right);
        }
    }

    public static void Run<T>([NotNull] Span<T> array) 
        where T : unmanaged
    {
        if (array == Span<T>.Empty)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (!AdvInstructionSet.X86.IsSupportedAvx256)
        {
            MemoryExtensions.Sort(array);
            return;
        }

        // TODO: Improve this.
        fixed (T* arrayPtr = array)
        {
            T* left = arrayPtr;
            T* right = arrayPtr + array.Length - 1;
            Run(left, right);
        }
    }

    public static void Run<T>(T* start, int count) 
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(start, nameof(start));

        if (!AdvInstructionSet.X86.IsSupportedAvx256)
        {
            MemoryExtensions.Sort(new Span<T>(start, count));
            return;
        }

        Run(start, start + count - 1);
    }
}
