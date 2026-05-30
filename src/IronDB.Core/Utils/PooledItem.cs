using IronDB.Core.Threading;

namespace IronDB.Core.Utils;

public abstract class PooledItem : IDisposable
{
    public MultipleUseFlag InUse = new();
    public DateTime InPoolSince;

    public abstract void Dispose();
}