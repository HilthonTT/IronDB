namespace IronDB.StorageEngine.Debugging;

public sealed class DetailedStorageReport
{
    //public InMemoryStorageState InMemoryState { get; set; } = default!;

    public DataFileReport DataFile { get; set; } = default!;

    public JournalsReport Journals { get; set; } = default!;

    public List<TempBufferReport> TempBuffers { get; set; } = [];

    public List<TreeReport> Trees { get; set; } = [];

    //public List<TableReport> Tables { get; set; } = [];

    public PreAllocatedBuffersReport PreAllocatedBuffers { get; set; } = default!;

    public string TotalEncryptionBufferSize { get; set; } = string.Empty;
}
