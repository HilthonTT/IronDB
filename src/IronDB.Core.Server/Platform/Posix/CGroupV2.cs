using System.Text.RegularExpressions;

namespace IronDB.Core.Server.Platform.Posix;

public sealed class CGroupV2 : CGroup
{
    private bool _hasMemoryPeakFile = true;

    protected override string MemoryLimitFileName => "memory.max";
    protected override string MemoryUsageFileName => "memory.current";
    protected override string MaxMemoryUsageFileName => "memory.peak";

    protected override string FindCGroupPathForMemoryInternal() => FindCGroupPath();

    protected override bool IsControllerGroupsAvailable(string subsysName)
    {
        // /proc/cgroups only lists cgroup v1 controllers, which may not include the memory
        // controller on kernels where memory is managed exclusively by cgroup v2 (e.g. Azure Linux).
        // For cgroup v2, we check the cgroup.controllers file at the process's cgroup path instead.
        try
        {
            string cgroupPath = FindCGroupPath();
            if (cgroupPath == null)
                return false;

            string controllersFilePath = Path.Combine(cgroupPath, "cgroup.controllers");
            if (File.Exists(controllersFilePath) == false)
                return false;

            string controllers = File.ReadAllText(controllersFilePath).Trim();
            foreach (string controller in controllers.Split(' '))
            {
                if (controller == subsysName)
                    return true;
            }

            return false;
        }
        catch (Exception e)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info($"Failed to check cgroup v2 controller availability for '{subsysName}', falling back to /proc/cgroups", e);

            return base.IsControllerGroupsAvailable(subsysName);
        }
    }

    private static string FindCGroupPath()
    {
        FindHierarchyMount(out var mountRoot, out var mountPath);
        var pathForSubsystem = FindCGroupPathForSubsystem();

        return CombinePaths(mountRoot, mountPath, pathForSubsystem);
    }
    private static void FindHierarchyMount(out string mountRoot, out string mountPath)
    {
        foreach (var match in FindHierarchyMount())
        {
            mountRoot = match.Groups["mountroot"].Value;
            mountPath = match.Groups["mountpath"].Value;
            return;
        }

        throw new CGroupException($"Couldn't find hierarchy mount in {PROC_MOUNTINFO_FILENAME}");
    }

    // 0::/user.slice/user-1000.slice/user@1000.service/apps.slice/apps-org.gnome.Terminal.slice/vte-spawn-d7794050-ce4a-451b-92c2-a2433019409e.scope
    private static readonly Regex FindCGroupPathForSubsystemReg = new Regex(@"^\d+::(?<path>.*)$", RegexOptions.Compiled);
    private static string? FindCGroupPathForSubsystem()
    {
        foreach (var line in File.ReadLines(PROC_SELF_CGROUP_FILENAME))
        {
            var match = FindCGroupPathForSubsystemReg.Match(line);
            if (match.Success == false)
                continue;

            return match.Groups["path"].Value;
        }

        return null;
    }

    public override long? GetMaxMemoryUsage()
    {
        try
        {
            return _hasMemoryPeakFile ? ReadValue(MaxMemoryUsageFileName) : null;
        }
        catch (Exception e)
        {
            if (e is FileNotFoundException)
                _hasMemoryPeakFile = false;

            if (Logger.IsInfoEnabled)
                Logger.Info($"Failed to get CGroup max memory usage from {MaxMemoryUsageFileName} - {GetType().Name}", e);
            return null;
        }
    }
}
