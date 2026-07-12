using IronDB.Core.Server.Unmanaged;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Server.Utils;

public unsafe struct GrowableBuffer<TNumber, TGrowth> : IDisposable
    where TGrowth : IBufferGrowth
    where TNumber : unmanaged, INumber<TNumber>
{
    private readonly TGrowth? _growthCalculator = default;
    private ByteStringContext? _context;
    private ByteString _buffer;
    private int _count;
    private int _maxAllocationInBytes;

    public GrowableBuffer()
    {
    }

    public readonly int Count => _count;

    public bool IsInitialized { get; private set; }

    public readonly int Capacity => IsInitialized ? _buffer.Length / sizeof(TNumber) : 0;

    public Span<TNumber> GetSpace()
    {
        if (_growthCalculator?.GrowingThresholdExceed(_count, _buffer.Length) == true)
        {
            Grow();
        }

        var space = _buffer.ToSpan<TNumber>().Slice(_count);
        if (space.IsEmpty)
        {
            ThrowCannotGrowAnyFurther();
        }

        return space;
    }

    public bool TryGetSpace(out Span<TNumber> space)
    {
        if (_growthCalculator?.GrowingThresholdExceed(_count, _buffer.Length) == true)
        {
            Grow();
        }

        space = _buffer.ToSpan<TNumber>().Slice(_count);
        return space.IsEmpty == false;
    }

    public void AddUsage(in int count) => _count += count;

    public void Truncate(in int newCount) => _count = newCount;

    public void Init(ByteStringContext context, in long initialSize, long maxAllocationInBytes = long.MaxValue)
    {
        if (!TryInit(context, initialSize, maxAllocationInBytes))
        {
            ThrowFailedToInitialize(initialSize, maxAllocationInBytes);
        }
    }

    public bool TryInit(ByteStringContext context, in long initialSize, long maxAllocationInBytes = long.MaxValue)
    {
        _context = context;

        long clampedMax = Math.Min(Math.Max(0, maxAllocationInBytes), IBufferGrowth.MaxBufferSizeInBytes);
        _maxAllocationInBytes = (int)(clampedMax - (clampedMax % Unsafe.SizeOf<TNumber>()));

        int initial;
        if (initialSize <= 0)
        {
            initial = _growthCalculator?.GetInitialSize(0) ?? 0;
        }
        else
        {
            long requested = initialSize >= IBufferGrowth.MaxBufferSizeInBytes / Unsafe.SizeOf<TNumber>()
                ? IBufferGrowth.MaxBufferSizeInBytes
                : initialSize * Unsafe.SizeOf<TNumber>();
            initial = (int)(requested - (requested % Unsafe.SizeOf<TNumber>()));
        }

        if (initial > _maxAllocationInBytes)
        {
            return false;
        }

        _context.Allocate(initial, out _buffer);
        IsInitialized = true;
        return true;
    }

    public void Dispose()
    {
        if (_buffer.HasValue)
        {
            _context?.Release(ref _buffer);
        }

        _buffer = default;
        IsInitialized = false;
    }

    [DoesNotReturn]
    private void ThrowCannotGrowAnyFurther()
    {
        throw new InvalidOperationException(
           $"GrowableBuffer cannot allocate more space. The buffer is full ({_count} items) and has reached its maximum allocation of {_maxAllocationInBytes} bytes.");
    }

    [DoesNotReturn]
    private static void ThrowFailedToInitialize(long initialSize, long maxAllocationInBytes)
    {
        throw new InvalidOperationException($"GrowableBuffer cannot allocate the requested buffer: initialSize={initialSize} items would exceed the configured maximum allocation of {maxAllocationInBytes} bytes.");
    }

    private void Grow()
    {
        if (_growthCalculator is null || _context is null)
        {
            return;
        }

        var newSize = _growthCalculator.GetNewSize(_buffer.Length);
        if (newSize > _maxAllocationInBytes)
        {
            newSize = _maxAllocationInBytes;
        }

        if (newSize <= _buffer.Length)
        {
            return;
        }

        _context.Allocate(newSize, out ByteString newBuffer);
        new Span<TNumber>(_buffer.Ptr, _count).CopyTo(new Span<TNumber>(newBuffer.Ptr, _count));
        _context.Release(ref _buffer);
        _buffer = newBuffer;
    }
}
