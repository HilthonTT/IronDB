namespace IronDB.Core.Server.Utils;

public sealed class DiskSpaceResult : DriveInfoBase
{
    public string? VolumeLabel { get; set; }

    public Size TotalFreeSpace { get; set; }

    public Size TotalSize { get; set; }
}
