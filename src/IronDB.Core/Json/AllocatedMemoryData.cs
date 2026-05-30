using System.Buffers;
using NativeMemory = IronDB.Core.Utils.NativeMemory;

#if MEM_GUARD
using IronDB.Platform;
#endif

namespace IronDB.Core.Json;

public sealed unsafe class AllocatedMemoryData
{
    public int SizeInBytes;
    public int ContextGeneration;

    public JsonOperationContext? Parent;
    public NativeMemory.ThreadStats? AllocatingThread;

    private MemoryManager? _memoryManager;

    public Memory<byte> AsMemory()
    {
        _memoryManager ??= new(this);
        return _memoryManager.Memory;
    }

    private class MemoryManager(AllocatedMemoryData parent) : MemoryManager<byte>
    {
        protected override void Dispose(bool disposing)
        {

        }

        public override Span<byte> GetSpan()
        {
            return new Span<byte>(parent.Address, parent.SizeInBytes);
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            return default;
        }

        public override void Unpin()
        {
        }
    }

    public AllocatedMemoryData(byte* address, int sizeInBytes)
    {
        SizeInBytes = sizeInBytes;
        Address = address;
    }

    public Span<byte> AsSpan()
    {
        return new Span<byte>(Address, SizeInBytes);
    }

#if MEM_GUARD_STACK || TRACK_ALLOCATED_MEMORY_DATA
    public string AllocatedBy = Environment.StackTrace;
    public string FreedBy;
#endif

#if !DEBUG
    public readonly byte* Address;
#else
    public bool IsLongLived;
    public bool IsReturned;

    private byte* _address;

    public byte* Address
    {
        get
        {
            if (IsLongLived == false &&
                Parent != null &&
                ContextGeneration != Parent.Generation ||
                IsReturned)
                ThrowObjectDisposedException();

            return _address;
        }

        private set
        {
            if (IsLongLived == false &&
                Parent != null &&
                ContextGeneration != Parent.Generation ||
                IsReturned)
                ThrowObjectDisposedException();

            _address = value;
        }
    }

    private void ThrowObjectDisposedException()
    {
        throw new ObjectDisposedException(nameof(AllocatedMemoryData));
    }

#endif
}
