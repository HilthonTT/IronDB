namespace IronDB.Core.Server.Platform.Posix;

public static class PosixHelper
{
    public static string FixLinuxPath(string path)
    {
        if (path != null)
        {
            var length = Path.GetPathRoot(path)!.Length;
            if (length > 0)
                path = "/" + path.Substring(length);
            path = path.Replace('\\', '/');
            path = path.Replace("/./", "/");
            path = path.Replace("//", "/");
        }

        return path!;
    }
}
