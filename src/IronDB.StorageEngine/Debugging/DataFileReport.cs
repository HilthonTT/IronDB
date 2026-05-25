using System.Drawing;

namespace IronDB.StorageEngine.Debugging;

public sealed class DataFileReport
{
    public override string ToString()
    {
        return $"{nameof(AllocatedSpaceInBytes)}: {new Size(AllocatedSpaceInBytes, SizeUnit.Bytes)}, {nameof(UsedSpaceInBytes)}: {new Size(UsedSpaceInBytes, SizeUnit.Bytes)}, {nameof(FreeSpaceInBytes)}: {new Size(FreeSpaceInBytes, SizeUnit.Bytes)}";
    }

    public long AllocatedSpaceInBytes { get; set; }
    public long UsedSpaceInBytes { get; set; }
    public long FreeSpaceInBytes { get; set; }
}
