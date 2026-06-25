using IronDB.Core;
using IronDB.Core.Binary;
using IronDB.Core.Json.Parsing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IronDB.Core.Json;

public unsafe interface IUnmanagedWriteBuffer : IDisposableQueryable, IDisposable
{
    int SizeInBytes { get; }
    void Write(byte[] buffer, int start, int count);
    void Write(byte* buffer, int length);
    void Write<T>(in T value) where T : unmanaged;
    void Write<T>(in ReadOnlySpan<T> buffer) where T : unmanaged;
    void WriteByte(byte data);
    void EnsureSingleChunk(JsonParserState state);
    void EnsureSingleChunk(out byte* ptr, out int size);
}
