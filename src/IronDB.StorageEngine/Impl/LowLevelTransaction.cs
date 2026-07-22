using IronDB.Core;
using IronDB.StorageEngine.Impl.Paging;
using static IronDB.StorageEngine.Data.Containers.Container;

namespace IronDB.StorageEngine.Impl;

public sealed unsafe class LowLevelTransaction : IPagerLevelTransactionState
{
    internal readonly PageLocator _pageLocator = default!;

    public Dictionary<AbstractPager, TransactionState> PagerTransactionState32Bits { get; set; } = [];

    public Dictionary<AbstractPager, CryptoTransactionState> CryptoPagerTransactionState { get; set; } = [];

    public Size AdditionalMemoryUsageSize => throw new NotImplementedException();

    public StorageEnvironment Environment => throw new NotImplementedException();

    public bool IsWriteTransaction => throw new NotImplementedException();

#pragma warning disable CS0067 // events are raised once commit/dispose are implemented
    public event Action<IPagerLevelTransactionState>? OnDispose;
    public event Action<IPagerLevelTransactionState>? BeforeCommitFinalization;
#pragma warning restore CS0067

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void EnsurePagerStateReference(ref PagerState state)
    {
        throw new NotImplementedException();
    }

    public void MarkTransactionAsFailed()
    {
        throw new NotImplementedException();
    }

    public string GetTxState()
    {
        throw new NotImplementedException();
    }
}
