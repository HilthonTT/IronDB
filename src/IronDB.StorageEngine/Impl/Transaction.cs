using IronDB.Core.Server.Unmanaged;
using System.Runtime.CompilerServices;

namespace IronDB.StorageEngine.Impl;

public sealed unsafe class Transaction
{
    private LowLevelTransaction _lowLevelTransaction = default!;

    public LowLevelTransaction LowLevelTransaction
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return _lowLevelTransaction; }
    }

    public ByteStringContext Allocator => _lowLevelTransaction.Allocator;
}
