using System.Runtime.InteropServices;

namespace IronDB.Core.Server.Platform.Posix;

public static class CGroupHelper
{
    public static readonly CGroup CGroup = GetCGroup();

    private static CGroup GetCGroup()
    {
        const uint TMPFS_MAGIC = 0x01021994;
        const uint CGROUP2_SUPER_MAGIC = 0x63677270;
        const string sysFsCgroupPath = "/sys/fs/cgroup";

        if (Syscall.statfs(sysFsCgroupPath, out var stats) != 0)
        {
            return new UnidentifiedCGroup($"Failed to get stats of {sysFsCgroupPath} because {Marshal.GetLastWin32Error()}");
        }

        return stats.f_type switch
        {
            TMPFS_MAGIC => new CGroupV1(),
            CGROUP2_SUPER_MAGIC => new CGroupV2(),
            _ => new UnidentifiedCGroup($"Didn't identify CGroup - {nameof(stats.f_type)}:{stats.f_type}")
        };
    }
}
