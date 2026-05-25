namespace IronDB.StorageEngine.Debugging;

public sealed class PreAllocatedBuffersReport
{
    public long AllocatedSpaceInBytes { get; set; }

    public long PreAllocatedBuffersSpaceInBytes { get; set; }

    public long NumberOfPreAllocatedPages { get; set; }

    public TreeReport AllocationTree { get; set; } = default!;

    public long OriginallyAllocatedSpaceInBytes { get; set; }
}
