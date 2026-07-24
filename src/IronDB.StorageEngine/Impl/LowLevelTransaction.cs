using IronDB.Core;
using IronDB.Core.Server.Unmanaged;
using IronDB.StorageEngine.Impl.Paging;
using System.Runtime.CompilerServices;
using static IronDB.StorageEngine.Data.Containers.Container;

namespace IronDB.StorageEngine.Impl;

public sealed unsafe class LowLevelTransaction : IPagerLevelTransactionState
{
    private const int InvalidScratchFile = -1;
    private TxState _txState;

    internal readonly PageLocator _pageLocator = default!;
    internal readonly Transaction _transaction = default!;

    internal Transaction Transaction => _transaction;

    public readonly AbstractPager DataPager = default!;

    public Dictionary<AbstractPager, TransactionState> PagerTransactionState32Bits { get; set; } = [];

    public Dictionary<AbstractPager, CryptoTransactionState> CryptoPagerTransactionState { get; set; } = [];

    public Size AdditionalMemoryUsageSize => throw new NotImplementedException();

    public StorageEnvironment Environment => throw new NotImplementedException();

    public bool IsWriteTransaction => throw new NotImplementedException();

    public ByteStringContext Allocator => _transaction.Allocator;

    public TransactionFlags Flags { get; }

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

    public Page AllocatePage(int numberOfPages, long? pageNumber = null, Page? previousPage = null, bool zeroPage = true)
    {
        throw new NotImplementedException();
    }

    internal void FreePageOnCommit(long pageNumber)
    {
        
    }

    public bool IsDirty(long p)
    {
        // TODO
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Page ModifyPage(long num)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Page GetPage(long pageNumber)
    {
        if (_txState != TxState.None)
        {
            ThrowObjectDisposed();
        }

        if (_pageLocator.TryGetReadOnlyPage(pageNumber, out Page result))
        {
            return result;
        }

        var p = GetPageInternal(pageNumber);

        _pageLocator.SetReadable(p);

        return p;
    }

    public T GetPageHeader<T>(long pageNumber) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void FreePage(long pageNumber
#if DEBUG
           , bool isOverflowShrink = false
#endif
       )
    {

    }

    internal void BreakLargeAllocationToSeparatePages(long pageNumber)
    {

    }

    private Page GetPageInternal(long pageNumber)
    {
        throw new NotImplementedException();
    }

    private void ThrowObjectDisposed()
    {
        if (_txState.HasFlag(TxState.Disposed))
        {
            throw new ObjectDisposedException("Transaction is already disposed");
        }

        if (_txState.HasFlag(TxState.Errored))
        {
            throw new InvalidDataException("The transaction is in error state, and cannot be used further");
        }

        throw new ObjectDisposedException("Transaction state is invalid: " + _txState);
    }

    [Flags]
    private enum TxState
    {
        None,
        Disposed = 1,
        Errored = 2
    }
}
