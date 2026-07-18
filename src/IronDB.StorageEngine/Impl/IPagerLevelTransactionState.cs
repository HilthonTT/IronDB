using IronDB.Core;
using IronDB.StorageEngine.Impl.Paging;
using static IronDB.StorageEngine.Data.Containers.Container;

namespace IronDB.StorageEngine.Impl;

public interface IPagerLevelTransactionState : IDisposable
{
    Dictionary<AbstractPager, TransactionState> PagerTransactionState32Bits { get; set; }

    Dictionary<AbstractPager, CryptoTransactionState> CryptoPagerTransactionState { get; set; }

    Size AdditionalMemoryUsageSize { get; }

    event Action<IPagerLevelTransactionState>? OnDispose;

    event Action<IPagerLevelTransactionState>? BeforeCommitFinalization;

    void EnsurePagerStateReference(ref PagerState state);

    StorageEnvironment Environment { get; }

    bool IsWriteTransaction { get; }
}