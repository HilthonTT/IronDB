using IronDB.StorageEngine.Impl;

namespace IronDB.StorageEngine.Data.Lookups;

public sealed unsafe partial class Lookup<TLookupKey> : IPrepareForCommit
    where TLookupKey : struct, ILookupKey
{
    private const int EncodingBufferSize = sizeof(long) + sizeof(long) + 1;

    private LowLevelTransaction? _llt;
    private LookupState _state;
    private int _treeStructureVersion;

    public void PrepareForCommit()
    {
        throw new NotImplementedException();
    }
}
