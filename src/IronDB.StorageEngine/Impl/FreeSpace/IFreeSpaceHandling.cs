using IronDB.Core.Json.Parsing;

namespace IronDB.StorageEngine.Impl.FreeSpace;

public interface IFreeSpaceHandling
{
    long? TryAllocateFromFreeSpace(LowLevelTransaction tx, int num);
    List<long> AllPages(LowLevelTransaction tx);
    int GetFreePagesCount(LowLevelTransaction txLowLevelTransaction);
    List<DynamicJsonValue> FreeSpaceSnapshot(LowLevelTransaction tx, bool hex);
    void FreePage(LowLevelTransaction tx, long pageNumber);
    long GetFreePagesOverhead(LowLevelTransaction tx);
    IEnumerable<long> GetFreePagesOverheadPages(LowLevelTransaction tx);
    Dictionary<long, FreeSpaceHandling.SectionMetadata> GetMaxConsecutiveRangePerSection(LowLevelTransaction tx);
    void OnRollback();
    FreeSpaceHandlingDisabler Disable();
}
