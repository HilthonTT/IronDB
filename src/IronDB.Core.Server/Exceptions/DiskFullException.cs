using IronDB.Core.Utils;

namespace IronDB.Core.Server.Exceptions;

public sealed class DiskFullException : IOException
{
    public DiskFullException()
    {
    }

    public string? DirectoryPath { get; private set; }

    public long CurrentFreeSpace { get; private set; }

    public DiskFullException(string filePath, long requestedFileSize, long? freeSpace, string msg)
        : base(
            $"There is not enough space to set the size of file {filePath} to {Sizes.Humane(requestedFileSize)}. " +
            $"Currently available space: {Sizes.Humane(freeSpace) ?? "N/A"}. Error Message: {msg}"
        )
    {
        DirectoryPath = Path.GetDirectoryName(filePath);
        CurrentFreeSpace = freeSpace ?? requestedFileSize - 1;
    }

    public DiskFullException(string message) : base(message)
    {
    }

    public DiskFullException(string message, Exception exception) : base(message, exception)
    {
    }
}
