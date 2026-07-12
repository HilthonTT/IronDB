using IronDB.Core.Server.Unmanaged;

namespace IronDB.Core.Server.Utils;

public unsafe interface IBufferGrowth
{
    public static readonly int MaxBufferSizeInBytes = int.MaxValue - sizeof(ByteStringStorage);

    public int GetInitialSize(in long initialSize);
    public int GetNewSize(in int currentSizeInBytes);
    public bool GrowingThresholdExceed(in int count, in int sizeInBytes);
}