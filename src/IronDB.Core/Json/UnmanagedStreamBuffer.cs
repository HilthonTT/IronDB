#nullable disable warnings
// Stub buffer — fields kept for the eventual port; suppress "never used" until implementation lands.

#pragma warning disable CS0169, CS0649

using IronDB.Core.Json.Parsing;

namespace IronDB.Core.Json;

internal unsafe struct UnmanagedStreamBuffer : IUnmanagedWriteBuffer
{
    private readonly Stream _stream;
    private int _sizeInBytes;
    private int Used;
    private readonly JsonOperationContext.MemoryBuffer _buffer;
    private readonly JsonOperationContext.MemoryBuffer.ReturnBuffer _returnBuffer;
    private bool _isDisposed;

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
