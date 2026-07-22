using IronDB.Core;
using IronDB.StorageEngine.Data.Containers;

namespace IronDB.StorageEngine.Impl.Paging;

public sealed class TempPagerTransaction : IPagerLevelTransactionState
{
    private readonly bool _isWriteTransaction = false;

    public Dictionary<AbstractPager, Container.TransactionState> PagerTransactionState32Bits { get; set; } = [];

    public Dictionary<AbstractPager, CryptoTransactionState> CryptoPagerTransactionState { get; set; } = [];

    public Size AdditionalMemoryUsageSize
    {
        get
        {
            var cryptoTransactionStates = CryptoPagerTransactionState;
            if (cryptoTransactionStates is null)
            {
                return new Size(0, SizeUnit.Bytes);
            }

            var total = 0L;
            foreach (var state in cryptoTransactionStates.Values)
            {
                total += state.TotalCryptoBufferSize;
            }

            return new Size(total, SizeUnit.Bytes);
        }
    }

    public StorageEnvironment? Environment => null;

    public bool IsWriteTransaction => _isWriteTransaction;

    public event Action<IPagerLevelTransactionState>? OnDispose = null;

    public event Action<IPagerLevelTransactionState>? BeforeCommitFinalization = null;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void EnsurePagerStateReference(ref PagerState state)
    {
        throw new NotImplementedException();
    }
}
