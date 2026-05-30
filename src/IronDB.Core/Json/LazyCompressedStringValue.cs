// STUB: minimal surface to satisfy compiler. Methods throw NotImplementedException.

namespace IronDB.Core.Json;

public sealed unsafe class LazyCompressedStringValue
{
    public byte* Buffer;
    public int UncompressedSize;
    public int CompressedSize;

    private string? _string;
    private JsonOperationContext? _context;

    public LazyCompressedStringValue(
        string? str,
        byte* buffer,
        int uncompressedSize,
        int compressedSize,
        JsonOperationContext context)
    {
        _string = str;
        Buffer = buffer;
        UncompressedSize = uncompressedSize;
        CompressedSize = compressedSize;
        _context = context;
    }

    public LazyStringValue ToLazyStringValue() => throw new NotImplementedException();

    public override string ToString() => _string ?? string.Empty;

    public static implicit operator string(LazyCompressedStringValue value) => value.ToString();
}
