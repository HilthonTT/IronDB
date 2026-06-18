using Microsoft.IO;

namespace IronDB.Core.Json;

internal sealed class RecyclableMemoryStreamFactory
{
    private static readonly RecyclableMemoryStreamManager Manager = new(new RecyclableMemoryStreamManager.Options
    {
        AggressiveBufferReturn = true,
        MaximumBufferSize = Constants.Size.Megabyte,
        MaximumSmallPoolFreeBytes = 256 * Constants.Size.Megabyte,
        MaximumLargePoolFreeBytes = 128 * Constants.Size.Megabyte,
        ThrowExceptionOnToArray = true,
        LargeBufferMultiple = 64 * Constants.Size.Kilobyte,
        BlockSize = 32 * Constants.Size.Kilobyte
    });

    public static RecyclableMemoryStream GetRecyclableStream() =>
        Manager.GetStream(Guid.Empty);

    public static RecyclableMemoryStream GetRecyclableStream(ReadOnlySpan<byte> buffer) =>
        Manager.GetStream(Guid.Empty, null, buffer);

    public static RecyclableMemoryStream GetRecyclableStream(long requiredSize) =>
        Manager.GetStream(Guid.Empty, null, requiredSize);

    public static MemoryStream GetMemoryStream() => new();
}
