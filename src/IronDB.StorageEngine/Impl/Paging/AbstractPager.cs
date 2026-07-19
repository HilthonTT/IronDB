namespace IronDB.StorageEngine.Impl.Paging;

public abstract unsafe class AbstractPager
{
    public string FileName => throw new NotImplementedException();

    public StorageEnvironmentOptions Options => throw new NotImplementedException();

    internal virtual void ReleaseAllocationInfo(byte* baseAddress, long size)
    {
        throw new NotImplementedException();
    }

    public virtual void ProtectPageRange(byte* start, ulong size)
    {
    }

    public virtual void UnprotectPageRange(byte* start, ulong size)
    {
    }
}
