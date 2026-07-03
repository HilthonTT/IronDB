namespace IronDB.Core.Server.Unmanaged;

/// <summary>
/// This class implements a direct allocator, mostly used for testing.   
/// </summary>
public readonly struct ByteStringDirectAllocator : IByteStringAllocator
{
    public UnmanagedGlobalSegment Allocate(int size, Action allocationFailure)
    {
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
        memory.Dispose();
    }
}
