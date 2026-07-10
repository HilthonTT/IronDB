using IronDB.Core.Logging;
using IronDB.Core.Server.Logging;
using System.Text.RegularExpressions;

namespace IronDB.Core.Server.Platform.Posix;

public abstract class CGroup
{
    protected static readonly IIronLogger Logger = IronLogManager.Instance.GetLoggerForIronServer<CGroup>();

    public const string PROC_SELF_CGROUP_FILENAME = "/proc/self/cgroup";
    public const string PROC_MOUNTINFO_FILENAME = "/proc/self/mountinfo";
    public const string PROC_CGROUPS_FILENAME = "/proc/cgroups";

    private const string MEMORY_CONTROLLER_NAME = "memory";

    protected abstract string? MemoryLimitFileName { get; }
    protected abstract string? MemoryUsageFileName { get; }
    protected abstract string? MaxMemoryUsageFileName { get; }


    private Lazy<CachedPath> _groupPathForMemory;

    private sealed class CachedPath
    {
        public DateTime ExpiryTime { get; private set; }
        public string? Path { get; }

        public CachedPath(string? path, DateTime expiryTime)
        {
            Path = path;
            ExpiryTime = expiryTime;
        }

        public void Deprecate()
        {
            ExpiryTime = DateTime.MinValue;
        }
    }

    protected CGroup()
    {
        _groupPathForMemory = CreateNewLazyCachedPath();
    }

    public virtual long? GetMaxMemoryUsage()
    {
        try
        {
            return ReadValue(MaxMemoryUsageFileName);
        }
        catch (Exception e)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info($"Failed to get CGroup max memory usage from {MaxMemoryUsageFileName} - {GetType().Name}", e);
            return null;
        }
    }
    public long? GetPhysicalMemoryUsage()
    {
        try
        {
            return ReadValue(MemoryUsageFileName);
        }
        catch (Exception e)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info($"Failed to get CGroup current memory usage from {MemoryUsageFileName} - {GetType().Name}", e);
            return null;
        }
    }
    public long? GetPhysicalMemoryLimit()
    {
        try
        {
            return ReadValue(MemoryLimitFileName, CheckLimitValues);
        }
        catch (Exception e)
        {
            if (Logger.IsInfoEnabled)
                Logger.Info($"Failed to get CGroup memory limit from {MemoryLimitFileName} - {GetType().Name}", e);
            return null;
        }
    }

    private Lazy<CachedPath> GetGroupPathForMemory()
    {
        var groupPathForMemory = _groupPathForMemory;
        if (DateTime.UtcNow > groupPathForMemory.Value.ExpiryTime)
        {
            var newVal = CreateNewLazyCachedPath();
            Interlocked.CompareExchange(ref _groupPathForMemory, newVal, groupPathForMemory);
        }

        return _groupPathForMemory;
    }

    private Lazy<CachedPath> CreateNewLazyCachedPath()
    {
        return new Lazy<CachedPath>(() =>
        {
            string? path = null;
            try
            {
                if (IsControllerGroupsAvailable(MEMORY_CONTROLLER_NAME) == false)
                    return new CachedPath(null, DateTime.MaxValue);
                path = FindCGroupPathForMemory();
            }
            catch (Exception e)
            {
                if (Logger.IsWarnEnabled)
                    Logger.Warn("Failed to get CGroup path for memory", e);
            }

            return new CachedPath(path, DateTime.UtcNow + TimeSpan.FromMinutes(1));
        });
    }

    private static bool CheckLimitValues(string textValue, out long? value)
    {
        //'max' stands for unlimited 
        if (textValue.StartsWith("max"))
        {
            value = long.MaxValue;
            return true;
        }

        value = null;
        return false;
    }

    protected long? ReadValue(string? file, CheckSpecialValues? checkSpecialValues = null, bool retry = true)
    {
        var basePath = GetGroupPathForMemory();
        if (basePath.Value.Path is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(file))
        {
            return null;
        }

        try
        {
            var path = Path.Combine(basePath.Value.Path, file);
            return ReadMemoryValueFromFile(path, checkSpecialValues);
        }
        catch (Exception e)
        {
            if (e is not DirectoryNotFoundException || retry == false)
                throw;

            //If the cgroup changed and the old cgroup was removed
            basePath.Value.Deprecate();
            return ReadValue(file, checkSpecialValues, false);
        }
    }

    private string? FindCGroupPathForMemory()
    {
        return FindCGroupPathForMemoryInternal();
    }

    protected abstract string? FindCGroupPathForMemoryInternal();

    //51 34 0:46 / /sys/fs/cgroup/hugetlb rw,nosuid,nodev,noexec,relatime shared:27 - cgroup cgroup rw,hugetlb
    private static readonly Regex FindHierarchyMountReg = new(@"^(?:\S+\s+){3}(?<mountroot>\S+)\s+(?<mountpath>\S+).* - (?<filesystemType>\S+)\s+\S+\s+(?:(?<options>[^,]+),?)+$", RegexOptions.Compiled);
    protected static IEnumerable<Match> FindHierarchyMount()
    {
        foreach (var line in File.ReadLines(PROC_MOUNTINFO_FILENAME))
        {
            var match = FindHierarchyMountReg.Match(line);
            if (match.Success == false)
                continue;

            if (match.Groups["filesystemType"].Value.StartsWith("cgroup") == false)
                continue;

            yield return match;
        }
    }

    protected static string CombinePaths(string mountRoot, string mountPath, string? pathForSubsystem)
    {
        var toAppend = mountRoot.Length == 1 || pathForSubsystem?.StartsWith(mountRoot) == false
            ? pathForSubsystem
            : pathForSubsystem?[mountRoot.Length..];

        return mountPath + toAppend;
    }

    //memory 0	205	1
    //cpu    2  232 1
    private static readonly Regex FindControllerGroupsAvailability = new(@"^(?<subsys_name>[\w|_]+)\s+(?<hierarchy>\d+)\s+(?<num_cgroups>\d+)\s+(?<enabled>[1|0])$", RegexOptions.Compiled);
    protected virtual bool IsControllerGroupsAvailable(string subsysName)
    {
        foreach (string line in File.ReadLines(PROC_CGROUPS_FILENAME))
        {
            var match = FindControllerGroupsAvailability.Match(line);
            if (match.Success == false)
                continue;

            if (match.Groups["subsys_name"].Value.Equals(subsysName) == false)
                continue;

            return match.Groups["enabled"].Value == "1";
        }

        return false;
    }

    protected delegate bool CheckSpecialValues(string textValue, out long? value);
    private static long? ReadMemoryValueFromFile(string fileName, CheckSpecialValues? checkSpecialValues)
    {
        var txt = File.ReadAllText(fileName);
        if (checkSpecialValues is not null && checkSpecialValues(txt, out var value))
        {
            return value;
        }
        var result = Convert.ToInt64(txt);
        if (result <= 0)
        {
            return null;
        }

        return result;
    }

    #region for_test
    public bool ForTestIsControllerMemoryGroupsAvailable() => IsControllerGroupsAvailable(MEMORY_CONTROLLER_NAME);
    public string? ForTestFindCGroupPathForMemory() => FindCGroupPathForMemoryInternal();
    #endregion
}
