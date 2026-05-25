namespace IronDB.StorageEngine.Debugging;

public sealed class CommitStats
{
    public int NumberOfModifiedPages { get; set; }

    public int NumberOf4KbsWrittenToDisk { get; set; }
}
