using IronDB.StorageEngine.Data.Fixed;
using IronDB.StorageEngine.Data.Tables;
using IronDB.StorageEngine.Impl;
using IronDB.StorageEngine.Impl.Paging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace IronDB.StorageEngine.Data.BTrees;

public unsafe partial class Tree
{
    private readonly LowLevelTransaction _llt = default!;


    private int _directAddUsage;

    public Slice Name { get; private set; }

    public LowLevelTransaction Llt => _llt;

    internal byte* DirectRead(Slice key)
    {
        throw new NotImplementedException();
    }

    public DirectAddScope DirectAdd(Slice key, int len, out byte* ptr)
    {
        return DirectAdd(key, len, TreeNodeFlags.Data, out ptr);
    }

    public DirectAddScope DirectAdd(Slice key, int len, TreeNodeFlags nodeType, out byte* ptr, bool populateDataPtr = true)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectAddScope AddKeyOnly(Slice key)
    {
        return DirectAdd(key, 0, TreeNodeFlags.Data, out _, populateDataPtr: false);
    }

    public FixedSizeTree FixedTreeFor(Slice key, byte valSize = 0)
    {
        throw new NotImplementedException();
    }

    public void Delete(Slice key)
    {

    }

    public readonly struct DirectAddScope : IDisposable
    {
        private readonly Tree _parent;

        public DirectAddScope(Tree parent)
        {
            _parent = parent;
            if (_parent._directAddUsage++ != 0)
            {
                ThrowScopeAlreadyOpen();
            }

#if VALIDATE_DIRECT_ADD_STACKTRACE
                _parent._allocationStacktrace = Environment.StackTrace;
#endif
        }

        public void Dispose()
        {
            _parent._directAddUsage--;
        }


        [DoesNotReturn]
        private void ThrowScopeAlreadyOpen()
        {
            var message = $"Write operation already requested on a tree name: {_parent}. " +
                          $"{nameof(DirectAdd)} method cannot be called recursively while the scope is already opened.";

#if VALIDATE_DIRECT_ADD_STACKTRACE
                message += Environment.NewLine + _parent._allocationStacktrace;
#endif

            throw new InvalidOperationException(message);
        }
    }

    [DoesNotReturn]
    private static void ThrowUnknownNodeTypeAddOperation(TreeNodeFlags nodeType)
    {
        throw new NotSupportedException("Unknown node type for direct add operation: " + nodeType);
    }

    [DoesNotReturn]
    private static void ThrowInvalidKeySize(Slice key)
    {
        throw new ArgumentException(
            $"Key size is too big, must be at most {AbstractPager.MaxKeySize} bytes, but was {(key.Size + AbstractPager.RequiredSpaceForNewNode)}",
            nameof(key));
    }

    [DoesNotReturn]
    private void ThrowCannotAddInReadTx()
    {
        throw new ArgumentException("Cannot add a value in a read only transaction on " + Name + " in " + _llt.Flags);
    }

    [DoesNotReturn]
    public static void ThrowAttemptToFreePageToNewPageAllocator(Slice treeName, long pageNumber)
    {
        throw new InvalidOperationException($"Attempting to free page #{pageNumber} of '{treeName}' tree to {nameof(NewPageAllocator)} while it wasn't allocated by it");
    }

    [DoesNotReturn]
    public static void ThrowAttemptToFreeIndexPageToFreeSpaceHandling(Slice treeName, long pageNumber)
    {
        throw new InvalidOperationException($"Attempting to free page #{pageNumber} of '{treeName}' index tree to the free space handling. The page was allocated by {nameof(NewPageAllocator)} so it needs to be returned there.");
    }
}
