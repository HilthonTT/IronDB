namespace IronDB.StorageEngine.Impl.Paging;

public interface I4KbBatchWrites : IDisposable
{
    unsafe void Write(long posBy4Kbs, int numberOf4Kbs, byte* source);
}
