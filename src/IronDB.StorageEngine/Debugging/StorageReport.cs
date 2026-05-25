namespace IronDB.StorageEngine.Debugging;

public sealed class StorageReport
{
    public DataFileReport DataFile { get; set; } = default!;

    public List<JournalReport> Journals { get; set; } = [];

    public List<TempBufferReport> TempFiles { get; set; } = [];

    public int CountOfTrees { get; set; }

    public int CountOfTables { get; set; }
}
