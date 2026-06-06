namespace IronDB.Core.LowMemory;

internal sealed class LowMemEventDetails
{
    public LowMemReason Reason;
    public long FreeMem;
    public DateTime Time;
    public long CurrentCommitCharge { get; set; }
    public long PhysicalMem { get; set; }
    public long TotalUnmanaged { get; set; }
    public long TotalScratchDirty { get; set; }
    public long LowMemThreshold { get; set; }
}