namespace IronDB.Core.Server.Unmanaged;

public interface IByteStringAllocator
{
    UnmanagedGlobalSegment Allocate(int size, Action allocationFailure);

    void Free(UnmanagedGlobalSegment memory);
}
