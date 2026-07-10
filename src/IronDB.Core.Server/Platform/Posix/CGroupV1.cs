using System.Text.RegularExpressions;

namespace IronDB.Core.Server.Platform.Posix;

public sealed class CGroupV1 : CGroup
{
    protected override string MemoryLimitFileName => "memory.limit_in_bytes";
    protected override string MemoryUsageFileName => "memory.usage_in_bytes";
    protected override string MaxMemoryUsageFileName => "memory.max_usage_in_bytes";

    protected override string FindCGroupPathForMemoryInternal()
    {
        return FindCGroupPath(l => l.Contains("memory"));
    }

    private static string FindCGroupPath(Predicate<IEnumerable<string>> isSubSystem)
    {
        FindHierarchyMount(isSubSystem, out var mountRoot, out var mountPath);
        var pathForSubsystem = FindCGroupPathForSubsystem(isSubSystem);
        return CombinePaths(mountRoot, mountPath, pathForSubsystem);
    }
    private static void FindHierarchyMount(Predicate<IEnumerable<string>> isSubSystem, out string mountRoot, out string mountPath)
    {
        foreach (var match in FindHierarchyMount())
        {
            if (!isSubSystem(match.Groups["options"].Captures.Select(x => x.Value)))
            {
                continue;
            }

            mountRoot = match.Groups["mountroot"].Value;
            mountPath = match.Groups["mountpath"].Value;
            return;
        }

        throw new CGroupException($"Couldn't find hierarchy mount in {PROC_MOUNTINFO_FILENAME}");
    }

    // 8:memory:/user.slice/user-1000.slice/user@1000.service
    // 7:cpu,cpuacct:/user.slice
    private static readonly Regex FindCGroupPathForSubsystemReg = new(@"^\d+:(?:(?<subsystem_list>[^,:]+),?)+:(?<path>.*)$", RegexOptions.Compiled);
   
    private static string? FindCGroupPathForSubsystem(Predicate<IEnumerable<string>> isSubSystem)
    {
        foreach (var line in File.ReadLines(PROC_SELF_CGROUP_FILENAME))
        {
            var match = FindCGroupPathForSubsystemReg.Match(line);
            if (!match.Success)
            {
                throw new CGroupException($"Failed to parse cgroup info file contents - {line}.");
            }

            if (isSubSystem(match.Groups["subsystem_list"].Captures.Select(x => x.Value)) == false)
            {
                continue;
            }

            return match.Groups["path"].Value;
        }

        return null;
    }
}
