using IronDB.StorageEngine.Data.Lookups;

namespace IronDB.StorageEngine.Data.Containers;

public readonly unsafe ref struct Container
{
    private const int InvalidId = -1;
    private const int MinimumAdditionalFreeSpaceConsider = 64;
    private const int NumberOfReservedEntries = 4;  // all pages, free pages, number of entries, next free page

    public sealed class TransactionState(ContainerId containerId)
    {
        // page -> page-level-metadata
        public Dictionary<long, long> FreeListAdditions = [];
        public HashSet<long> FreeListRemovals = [];

        public HashSet<long> Removals = [];
        public HashSet<long> Additions = [];

        public Dictionary<long, long> LastFreePageByPageLevelMetadata = [];

        public ContainerId ContainerId = containerId;
#pragma warning disable CS0169 // fields are placeholders until the container implementation lands
        private Lookup<Int64LookupKey>? _allPages;
        private Lookup<Int64LookupKey>? _freePages;
#pragma warning restore CS0169
    }
}