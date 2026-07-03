using IronDB.Core.Utils;

namespace IronDB.Core.Server.Unmanaged;

public sealed unsafe class UnmanagedGlobalSegment : PooledItem
{
    public byte* Segment;
    public readonly int Size;
    private readonly NativeMemory.ThreadStats _thread;

    public UnmanagedGlobalSegment(int size)
    {
        Size = size;
        Segment = NativeMemory.AllocateMemory(size, out _thread);
        InUse.Raise();
    }

    ~UnmanagedGlobalSegment()
    {
        try
        {
            if (Segment is null)
            {
                return;
            }
            NativeMemory.Free(Segment, Size, _thread);
            Segment = null;
        }
        catch (ObjectDisposedException)
        {
            // nothing that can be done here
        }
    }

    public override void Dispose()
    {
        if (Segment is null)
        {
            return;
        }

        lock (this)
        {
            if (Segment is null)
            {
                return;
            }

            NativeMemory.Free(Segment, Size, _thread);
            Segment = null;
            GC.SuppressFinalize(this);
        }
    }
}
