namespace IronDB.StorageEngine.Debugging;

public sealed class JournalReport
{
    public long Number { get; set; }
    public long AllocatedSpaceInBytes { get; set; }
    public long Available4Kbs { get; set; }
    public long LastTransaction { get; set; }
    public bool Flushed { get; set; }
}
