namespace IronDB.StorageEngine.Debugging;

public sealed class SizeReport
{
    public long DataFileInBytes { get; set; }

    public long JournalsInBytes { get; set; }

    public long TempBuffersInBytes { get; set; }

    public long TempRecyclableJournalsInBytes { get; set; }
}
