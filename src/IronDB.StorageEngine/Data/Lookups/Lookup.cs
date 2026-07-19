using IronDB.StorageEngine.Impl;

namespace IronDB.StorageEngine.Data.Lookups;

public sealed unsafe partial class Lookup<TLookupKey> : IPrepareForCommit
    where TLookupKey : struct, ILookupKey
{
    private const int EncodingBufferSize = sizeof(long) + sizeof(long) + 1;

#pragma warning disable CS0169 // fields are placeholders until the lookup implementation lands
    private LowLevelTransaction? _llt;
    private LookupState _state;
    private int _treeStructureVersion;
#pragma warning restore CS0169

    public void PrepareForCommit()
    {
        throw new NotImplementedException();
    }
}
