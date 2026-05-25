namespace IronDB.StorageEngine.Debugging;

public sealed class JournalsReport
{
    public long LastFlushedJournal { get; set; }
    public long TotalWrittenButUnsyncedBytes { get; set; }
    public long LastFlushedTransaction { get; set; }
    public List<JournalReport> Journals { get; set; } = [];
}
