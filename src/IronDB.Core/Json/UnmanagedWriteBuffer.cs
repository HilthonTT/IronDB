#nullable disable warnings
// Stub buffer — methods throw NotImplementedException until the full port lands.

using IronDB.Core.Json.Parsing;

namespace IronDB.Core.Json;

public unsafe struct UnmanagedWriteBuffer : IUnmanagedWriteBuffer
{
    public UnmanagedWriteBuffer(JsonOperationContext context, AllocatedMemoryData buffer)
    {
        _ = context;
        _ = buffer;
    }

    public int SizeInBytes => throw new NotImplementedException();

    public bool IsDisposed => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void EnsureSingleChunk(JsonParserState state)
    {
        throw new NotImplementedException();
    }

    public unsafe void EnsureSingleChunk(out byte* chunkPtr, out int size)
    {
        throw new NotImplementedException();
    }

    public void Write(byte[] buffer, int start, int count)
    {
        throw new NotImplementedException();
    }

    public unsafe void Write(byte* buffer, int length)
    {
        throw new NotImplementedException();
    }

    public void Write<T>(in T value) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void Write<T>(in ReadOnlySpan<T> buffer) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void WriteByte(byte data)
    {
        throw new NotImplementedException();
    }
}
