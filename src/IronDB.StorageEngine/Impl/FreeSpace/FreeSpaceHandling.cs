using IronDB.Core.Json.Parsing;
using System.Runtime.InteropServices;

namespace IronDB.StorageEngine.Impl.FreeSpace;

public sealed class FreeSpaceHandling : IFreeSpaceHandling
{
    internal const int NumberOfPagesInSection = 2048;

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct SectionMetadata()
    {
        public ushort Max = NumberOfPagesInSection;
        public ushort StartBits = NumberOfPagesInSection;
        public ushort EndBits = NumberOfPagesInSection;

        private readonly ushort Reserved = 0;
    }

    public List<long> AllPages(LowLevelTransaction tx)
    {
        throw new NotImplementedException();
    }

    public FreeSpaceHandlingDisabler Disable()
    {
        throw new NotImplementedException();
    }

    public void FreePage(LowLevelTransaction tx, long pageNumber)
    {
        throw new NotImplementedException();
    }

    public List<DynamicJsonValue> FreeSpaceSnapshot(LowLevelTransaction tx, bool hex)
    {
        throw new NotImplementedException();
    }

    public int GetFreePagesCount(LowLevelTransaction txLowLevelTransaction)
    {
        throw new NotImplementedException();
    }

    public long GetFreePagesOverhead(LowLevelTransaction tx)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<long> GetFreePagesOverheadPages(LowLevelTransaction tx)
    {
        throw new NotImplementedException();
    }

    public Dictionary<long, FreeSpaceHandling.SectionMetadata> GetMaxConsecutiveRangePerSection(LowLevelTransaction tx)
    {
        throw new NotImplementedException();
    }

    public void OnRollback()
    {
        throw new NotImplementedException();
    }

    public long? TryAllocateFromFreeSpace(LowLevelTransaction tx, int num)
    {
        throw new NotImplementedException();
    }
}
