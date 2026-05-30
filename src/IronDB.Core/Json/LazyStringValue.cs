// STUB: minimal surface to satisfy compiler. Methods throw NotImplementedException.

namespace IronDB.Core.Json;

public sealed unsafe class LazyStringValue : IDisposable
{
    public byte* Buffer;
    public int Size;
    public bool IsDisposed;
    public AllocatedMemoryData? AllocatedMemoryData;
    public int[]? EscapePositions;

    private string? _string;

    public LazyStringValue(string? str, byte* buffer, int size, JsonOperationContext context)
    {
        _string = str;
        Buffer = buffer;
        Size = size;
        _ = context;
    }

    public void Renew(string? str, byte* buffer, int size, JsonOperationContext context)
    {
        _string = str;
        Buffer = buffer;
        Size = size;
        IsDisposed = false;
        _ = context;
    }

    public void Reset() => throw new NotImplementedException();

    public void Dispose()
    {
        IsDisposed = true;
    }

    public ReadOnlySpan<char> AsSpan() => throw new NotImplementedException();

    public bool StartsWith(char value) => ToString().StartsWith(value);

    public bool StartsWith(string value) => ToString().StartsWith(value, StringComparison.Ordinal);

    public int Compare(byte* buffer, int size)
    {
        _ = buffer;
        _ = size;
        throw new NotImplementedException();
    }

    public override string ToString() => _string ?? string.Empty;

    public static implicit operator string(LazyStringValue value) => value.ToString();
}
