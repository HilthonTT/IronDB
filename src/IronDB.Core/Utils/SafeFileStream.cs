using Microsoft.Win32.SafeHandles;

namespace IronDB.Core.Utils;

internal static class SafeFileStream
{
    public static FileStream Create(string path, FileMode mode)
    {
        var fs = new FileStream(path, mode);
        AvoidKillingTheProcessWhenOutOfDiskSpace(fs);
        return fs;
    }

    public static FileStream Create(string path, FileMode mode, FileAccess access)
    {
        var file = new FileStream(path, mode, access);
        AvoidKillingTheProcessWhenOutOfDiskSpace(file);
        return file;
    }

    public static FileStream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
    {
        var file = new FileStream(path, mode, access, share, bufferSize, options);
        AvoidKillingTheProcessWhenOutOfDiskSpace(file);
        return file;
    }

    public static FileStream Create(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
    {
        var file = new FileStream(path, mode, access, share, bufferSize, useAsync);
        AvoidKillingTheProcessWhenOutOfDiskSpace(file);
        return file;
    }

    public static FileStream Create(string path, FileMode mode, FileAccess access, FileShare share)
    {
        var file = new FileStream(path, mode, access, share);
        AvoidKillingTheProcessWhenOutOfDiskSpace(file);
        return file;
    }

    public static FileStream Create(SafeFileHandle handle, FileAccess access)
    {
        var file = new FileStream(handle, access);
        AvoidKillingTheProcessWhenOutOfDiskSpace(file);
        return file;
    }

    private static void AvoidKillingTheProcessWhenOutOfDiskSpace(FileStream fs)
    {
        GC.SuppressFinalize(fs); // See: https://github.com/dotnet/corefx/issues/26734
    }
}
