using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace IronDB.Core;

internal static class PortableIntrinsics
{
    public static bool CanPrefetch
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
#if NET9_0_OR_GREATER
#pragma warning disable SYSLIB5003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            if (Sve.IsSupported)
            {
                return true;
            }
#pragma warning restore SYSLIB5003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#endif
#if NET7_0_OR_GREATER
            if (Sse.IsSupported)
            {
                return true;
            }
#endif
            return false;
        }
    }

    /// <summary>
    /// Prefetch a single cache line for temporal read (L1).
    /// x86: SSE PREFETCHT0.  ARM SVE: PRFM PLDL1KEEP.  Otherwise: no-op.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void PrefetchRead(void* address)
    {
#if NET7_0_OR_GREATER
        if (Sse.IsSupported)
        {
            Sse.Prefetch0(address);
            return;
        }
#endif
#if NET9_0_OR_GREATER
#pragma warning disable SYSLIB5003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        if (Sve.IsSupported)
        {
            Sve.Prefetch8Bit(Sve.CreateTrueMaskByte(), address, SvePrefetchType.LoadL1Temporal);
        }
#pragma warning restore SYSLIB5003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#endif
    }

    /// <summary>
    /// Prefetch a contiguous memory range for temporal read at 512-byte intervals.
    /// The stride primes the hardware sequential prefetcher across page boundaries.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void PrefetchRange(byte* address, int length)
    {
        if (!CanPrefetch)
        {
            return;
        }

        for (byte* p = address, end = address + length; p < end; p += 512)
        {
            PrefetchRead(p);
        }
    }
}
