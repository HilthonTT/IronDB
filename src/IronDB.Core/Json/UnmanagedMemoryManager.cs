using System.Buffers;

#if MEM_GUARD
using IronDB.Platform;
#endif

namespace IronDB.Core.Json;

internal sealed unsafe class UnmanagedMemoryManager : MemoryManager<byte>
{
    private readonly byte* _address;
    private readonly int _length;

    public UnmanagedMemoryManager(byte* pointer, int length)
    {
        _address = pointer;
        _length = length;
    }

    public override Memory<byte> Memory => CreateMemory(_length);

    public override Span<byte> GetSpan() => new Span<byte>(_address, _length);

    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if (elementIndex < 0 || elementIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));

        return new MemoryHandle(_address + elementIndex);
    }

    public override void Unpin()
    {
    }

    protected override void Dispose(bool disposing)
    {
    }
}