namespace IronDB.StorageEngine.Debugging;

public sealed class StreamDetails
{
    public string Name { get; set; } = string.Empty;

    public long Length { get; set; }

    public int Version { get; set; }

    public long NumberOfAllocatedPages { get; set; }

    public long AllocatedSpaceInBytes { get; set; }

    public TreeReport ChunksTree { get; set; } = default!;
}