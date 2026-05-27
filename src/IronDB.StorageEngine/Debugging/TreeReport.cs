namespace IronDB.StorageEngine.Debugging;

public sealed class TreeReport
{
    //public RootObjectType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public long PageCount { get; set; }

    public long NumberOfEntries { get; set; }

    public long BranchPages { get; set; }

    public long LeafPages { get; set; }

    public long OverflowPages { get; set; }

    public int Depth { get; set; }

    public double Density { get; set; }

    public MultiValuesReport MultiValues { get; set; } = default!;

    public long AllocatedSpaceInBytes { get; set; }

    public long UsedSpaceInBytes { get; set; }

    public StreamsReport Streams { get; set; } = default!;

    public Dictionary<int, int> BalanceHistogram { get; internal set; } = [];
}
