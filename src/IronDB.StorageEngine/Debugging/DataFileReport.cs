using IronDB.Core;
using System.Drawing;

namespace IronDB.StorageEngine.Debugging;

public sealed class DataFileReport
{
    public override string ToString()
    {
        return $"{nameof(AllocatedSpaceInBytes)}: {new Size((int)AllocatedSpaceInBytes, (int)SizeUnit.Bytes)}, {nameof(UsedSpaceInBytes)}: {new Size((int)UsedSpaceInBytes, (int)SizeUnit.Bytes)}, {nameof(FreeSpaceInBytes)}: {new Size((int)FreeSpaceInBytes, (int)SizeUnit.Bytes)}";
    }

    public long AllocatedSpaceInBytes { get; set; }
    public long UsedSpaceInBytes { get; set; }
    public long FreeSpaceInBytes { get; set; }
}
