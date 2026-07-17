using IronDB.Core.Server.Utils;

namespace IronDB.StorageEngine;

public sealed class DriveInfoByPath
{
    public DriveInfoBase BasePath { get; set; } = default!;

    public DriveInfoBase JournalPath { get; set; } = default!;

    public DriveInfoBase TempPath { get; set; } = default!;
}