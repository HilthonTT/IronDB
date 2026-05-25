namespace IronDB.StorageEngine.Debugging;

public sealed class StreamsReport
{
    public List<StreamDetails> Streams { get; set; } = [];

    public long NumberOfStreams { get; set; }

    public long TotalNumberOfAllocatedPages { get; set; }

    public long AllocatedSpaceInBytes { get; set; }
}
