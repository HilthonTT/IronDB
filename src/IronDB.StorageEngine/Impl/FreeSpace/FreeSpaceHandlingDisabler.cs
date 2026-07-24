namespace IronDB.StorageEngine.Impl.FreeSpace;

public sealed class FreeSpaceHandlingDisabler : IDisposable
{
    public int DisableCount;

    public void Dispose()
    {
        DisableCount--;
    }
}