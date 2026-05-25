namespace IronDB.StorageEngine.Debugging;

public sealed class TempBufferReport
{
    public string Name { get; set; } = string.Empty;

    public long AllocatedSpaceInBytes { get; set; }

    public TempBufferType Type { get; set; }
}
