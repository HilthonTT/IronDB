namespace IronDB.StorageEngine.Debugging;

public sealed class MultiValuesReport
{
    public long NumberOfEntries { get; set; }
    public long PageCount { get; set; }
    public long BranchPages { get; set; }
    public long LeafPages { get; set; }
    public long OverflowPages { get; set; }
}
