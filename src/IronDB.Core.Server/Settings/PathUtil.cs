using IronDB.Core.Platform;
using IronDB.Core.Server.Platform.Posix;
using System.Runtime.InteropServices;

namespace IronDB.Core.Server.Settings;

public sealed class PathUtil
{
    public const string LongPathPrefixWindows = @"\\?\";

    public static string ToFullPath(string inputPath, string? baseDataDirFullPath = null)
    {
        var path = Environment.ExpandEnvironmentVariables(inputPath);

        if (PlatformDetails.RunningOnPosix == false && path.StartsWith(@"\") == false ||
            PlatformDetails.RunningOnPosix && path.StartsWith(@"/") == false) // if relative path
            path = Path.Combine(baseDataDirFullPath ?? AppContext.BaseDirectory, path);

        var result = Path.IsPathRooted(path)
            ? path
            : Path.Combine(baseDataDirFullPath ?? AppContext.BaseDirectory, path);

        if (result.Length >= 260 &&
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
            result.StartsWith(LongPathPrefixWindows) == false)
            result = LongPathPrefixWindows + result;

        var resultRoot = Path.GetPathRoot(result);
        if (resultRoot != result && (result.EndsWith(@"\") || result.EndsWith("/")))
            result = result.TrimEnd('\\', '/');

        if (PlatformDetails.RunningOnPosix)
            result = PosixHelper.FixLinuxPath(result);

        return result != string.Empty || resultRoot == null ?
            Path.GetFullPath(result) :
            Path.GetFullPath(resultRoot); // it will unify directory separators and sort out parent directories
    }

    public static bool IsSubDirectory(string userPath, string rootPath)
    {
        var rootDirInfo = new DirectoryInfo(rootPath);
        var userDirInfo = new DirectoryInfo(userPath);

        var comparisonType = PlatformDetails.RunningOnPosix ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        if (string.Equals(userDirInfo.FullName, rootDirInfo.FullName, comparisonType))
            return true;

        while (userDirInfo.Parent != null)
        {
            if (string.Equals(userDirInfo.Parent.FullName, rootDirInfo.FullName, comparisonType))
            {
                return true;
            }

            userDirInfo = userDirInfo.Parent;
        }
        return false;
    }
}