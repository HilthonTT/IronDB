using IronDB.Core;
using IronDB.StorageEngine.Impl.Paging;
using static IronDB.StorageEngine.Data.Containers.Container;

namespace IronDB.StorageEngine.Impl;

public sealed unsafe class LowLevelTransaction : IPagerLevelTransactionState
{
    public Dictionary<AbstractPager, TransactionState> PagerTransactionState32Bits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Dictionary<AbstractPager, CryptoTransactionState> CryptoPagerTransactionState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Size AdditionalMemoryUsageSize => throw new NotImplementedException();

    public StorageEnvironment Environment => throw new NotImplementedException();

    public bool IsWriteTransaction => throw new NotImplementedException();

    public event Action<IPagerLevelTransactionState>? OnDispose;
    public event Action<IPagerLevelTransactionState>? BeforeCommitFinalization;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void EnsurePagerStateReference(ref PagerState state)
    {
        throw new NotImplementedException();
    }
}
