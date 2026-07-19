using IronDB.Core.LowMemory;
using IronDB.Core.Threading;
using IronDB.Core.Utils;
using System.Diagnostics.CodeAnalysis;

namespace IronDB.Core.Server.Unmanaged;

public unsafe struct ByteStringMemoryCache : IByteStringAllocator
{
    private static readonly LightWeightThreadLocal<StackHeader<UnmanagedGlobalSegment>> SegmentsPool =
        new(() => new StackHeader<UnmanagedGlobalSegment>());

    private static readonly SharedMultipleUseFlag LowMemoryFlag = new();
    private static readonly LowMemoryHandler LowMemoryHandlerInstance = new();

    public static readonly NativeMemoryCleaner<StackHeader<UnmanagedGlobalSegment>, UnmanagedGlobalSegment> Cleaner =
        new(typeof(ByteStringMemoryCache), _ => SegmentsPool.Values!, LowMemoryFlag, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

    static ByteStringMemoryCache()
    {
        ThreadLocalCleanup.ReleaseThreadLocalState += CleanForCurrentThread;

        LowMemoryNotification.Instance.RegisterLowMemoryHandler(LowMemoryHandlerInstance);
    }

    [ThreadStatic]
    private static int _minSize;


    public static void CleanForCurrentThread()
    {
        if (!SegmentsPool.IsValueCreated)
        {
            return; // nothing to do
        }

        var stack = SegmentsPool.Value;
        if (stack?.Head is null)
        {
            return;
        }

        _minSize = 0;
        var current = Interlocked.Exchange(ref stack.Head, null);
        while (current is not null)
        {
            current.Value?.Dispose();
            current = current.Next;
        }
    }

    public readonly UnmanagedGlobalSegment Allocate(int size, Action allocationFailure)
    {
        if (_minSize < size)
        {
            _minSize = size;
        }

        var stack = SegmentsPool.Value!;

        while (true)
        {
            StackNode<UnmanagedGlobalSegment>? current = stack.Head;
            if (current is null)
            {
                break;
            }

            if (Interlocked.CompareExchange(ref stack.Head, current.Next, current) != current)
            {
                continue;
            }

            var segment = current.Value;
            if (segment is null)
            {
                continue;
            }

            if (segment.Size >= size)
            {
                if (!segment.InUse.Raise())
                {
                    continue;
                }

                return segment;
            }

            // not big enough, so we'll discard it and create a bigger instance
            // it will go into the pool afterward and be available for future use
            segment.Dispose();
        }

        // have to allocate it directly
        try
        {
            return new UnmanagedGlobalSegment(size);
        }
        catch
        {
            allocationFailure?.Invoke();
            throw;
        }
    }

    public void Free(UnmanagedGlobalSegment memory)
    {
        if (memory.Segment is null)
        {
            ThrowInvalidMemorySegment();
        }

        if (_minSize > memory.Size)
        {
            memory.Dispose();
            return;
        }

        memory.InUse.Lower();
        memory.InPoolSince = DateTime.UtcNow;

        var stack = SegmentsPool.Value!;

        while (true)
        {
            StackNode<UnmanagedGlobalSegment>? current = stack.Head;

            var newHead = new StackNode<UnmanagedGlobalSegment> { Value = memory, Next = current };
            if (Interlocked.CompareExchange(ref stack.Head, newHead, current) == current)
            {
                return;
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowInvalidMemorySegment()
    {
        throw new InvalidOperationException("Attempt to return a memory segment that has already been disposed");
    }

    private sealed class LowMemoryHandler : ILowMemoryHandler
    {
        public void LowMemory(LowMemorySeverity lowMemorySeverity)
        {
            if (lowMemorySeverity != LowMemorySeverity.ExtremelyLow)
            {
                return;
            }

            if (LowMemoryFlag.Raise())
            {
                Cleaner.CleanNativeMemory(null);
            }
        }

        public void LowMemoryOver()
        {
            LowMemoryFlag.Lower();
        }
    }
}
