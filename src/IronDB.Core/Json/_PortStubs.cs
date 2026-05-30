// STUBS: minimal types added to satisfy the compiler while the full blittable JSON
// subsystem port is in progress. All members throw NotImplementedException — they exist
// solely to make the project compile. Remove this file as real implementations land.

using IronDB.Core.Json.Parsing;
using IronDB.Core.Threading;
using IronDB.Core.Utils;

namespace IronDB.Core.Json;

// ---------------------------------------------------------------------------
// Document builders
// ---------------------------------------------------------------------------

/// <summary>Builds blittable JSON documents from a parser stream. Stub.</summary>
public sealed class BlittableJsonDocumentBuilder : IDisposable
{
    public enum UsageMode
    {
        None,
        ToDisk,
    }

    internal IBlittableDocumentModifier? _modifier;

    public BlittableJsonDocumentBuilder(
        JsonOperationContext context,
        JsonParserState state,
        IJsonParser parser)
    {
        _ = context;
        _ = state;
        _ = parser;
    }

    public BlittableJsonDocumentBuilder(
        JsonOperationContext context,
        UsageMode mode,
        string debugTag,
        IJsonParser parser,
        JsonParserState state,
        IBlittableDocumentModifier? modifier = null)
    {
        _ = context;
        _ = mode;
        _ = debugTag;
        _ = parser;
        _ = state;
        _modifier = modifier;
    }

    public void Renew(string documentId, UsageMode mode) => throw new NotImplementedException();

    public void Reset() => throw new NotImplementedException();

    public void ReadObjectDocument() => throw new NotImplementedException();

    public void ReadArrayDocument() => throw new NotImplementedException();

    public bool Read() => throw new NotImplementedException();

    public void FinalizeDocument() => throw new NotImplementedException();

    public BlittableJsonReaderObject CreateReader() => throw new NotImplementedException();

    public BlittableJsonReaderArray CreateArrayReader(bool noCache) => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}

/// <summary>
/// Generic builder used to construct blittable documents programmatically against a write buffer.
/// Stub.
/// </summary>
public sealed class ManualBlittableJsonDocumentBuilder<TWriter>
    where TWriter : struct, IUnmanagedWriteBuffer
{
    public void WritePropertyName(LazyStringValue name) => throw new NotImplementedException();

    public void WriteValue(BlittableJsonToken token, object? value) => throw new NotImplementedException();
}

/// <summary>Hook invoked while building a blittable document to mutate it mid-parse. Stub.</summary>
public interface IBlittableDocumentModifier
{
}

// ---------------------------------------------------------------------------
// Reader companions
// ---------------------------------------------------------------------------

/// <summary>Reader for blittable JSON arrays. Stub.</summary>
public sealed unsafe class BlittableJsonReaderArray : IEnumerable<object>, IDisposable
{
    public BlittableJsonReaderArray(int position, BlittableJsonReaderBase parent, BlittableJsonToken type)
    {
        _ = position;
        _ = parent;
        _ = type;
    }

    public int Length => throw new NotImplementedException();

    public bool NoCache { get; set; }

    public DynamicJsonArray? Modifications;

    public object this[int index] => throw new NotImplementedException();

    public IEnumerator<object> GetEnumerator() => throw new NotImplementedException();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose() => throw new NotImplementedException();
}

/// <summary>Reader for blittable JSON vector values. Stub.</summary>
public sealed unsafe class BlittableJsonReaderVector : IDisposable
{
    public BlittableJsonReaderVector(byte* mem, int size, JsonOperationContext context)
    {
        _ = mem;
        _ = size;
        _ = context;
    }

    public int Length => throw new NotImplementedException();

    public DynamicJsonArray? Modifications;

    public object this[int index] => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}

/// <summary>Container for a blittable JSON document held by another object. Stub.</summary>
public interface IBlittableJsonContainer
{
    BlittableJsonReaderObject? BlittableJson { get; }
}

// ---------------------------------------------------------------------------
// Parser + property cache
// ---------------------------------------------------------------------------

/// <summary>Streaming JSON parser writing into unmanaged memory. Stub.</summary>
public sealed unsafe class UnmanagedJsonParser : IJsonParser
{
    public UnmanagedJsonParser(JsonOperationContext context, JsonParserState state, string debugTag)
    {
        _ = context;
        _ = state;
        _ = debugTag;
    }

    public int BufferOffset => throw new NotImplementedException();

    public void SetBuffer(JsonOperationContext.MemoryBuffer buffer) => throw new NotImplementedException();

    public void SetBuffer(JsonOperationContext.MemoryBuffer buffer, int offset, int size)
        => throw new NotImplementedException();

    public void SetBuffer(byte* buffer, int length) => throw new NotImplementedException();

    public bool Read() => throw new NotImplementedException();

    public void ValidatePool() => throw new NotImplementedException();

    public string GenerateErrorState() => throw new NotImplementedException();

    public IJsonParser.OnStringReadDelegate OnStringRead
    {
        set => throw new NotImplementedException();
    }

    public void Dispose() => throw new NotImplementedException();
}

/// <summary>Per-context cache of property names and ordering metadata. Stub.</summary>
public sealed class CachedProperties
{
    public CachedProperties(JsonOperationContext context) => _ = context;

    public void NewDocument() => throw new NotImplementedException();

    public void Renew() => throw new NotImplementedException();

    public void Reset() => throw new NotImplementedException();
}

/// <summary>Cache of path lookups for blittable document navigation. Stub.</summary>
public sealed class PathCache
{
    public void AcquirePathCache(
        out Dictionary<StringSegment, object> pathCache,
        out Dictionary<int, object> pathCacheByIndex)
    {
        pathCache = null!;
        pathCacheByIndex = null!;
        throw new NotImplementedException();
    }

    public void ReleasePathCache(
        Dictionary<StringSegment, object> pathCache,
        Dictionary<int, object> pathCacheByIndex)
        => throw new NotImplementedException();

    public void ClearUnreturnedPathCache() => throw new NotImplementedException();
}

/// <summary>Type-keyed value cache used by the object parser. Stub.</summary>
public sealed class ReplacementTypeCache<TValue>
{
    public ReplacementTypeCache(int capacity) => _ = capacity;

    public bool TryGet(Type type, out TValue value)
    {
        value = default!;
        return false;
    }

    public void Put(Type type, TValue value) => throw new NotImplementedException();
}

// ---------------------------------------------------------------------------
// Pooling infrastructure
// ---------------------------------------------------------------------------

/// <summary>Per-CPU bucket container. Stub.</summary>
public sealed class PerCoreContainer<T>
    where T : class
{
    public PerCoreContainer() { }
    public PerCoreContainer(int capacityPerCore) => _ = capacityPerCore;

    public bool TryPull(out T value)
    {
        value = null!;
        return false;
    }

    public bool TryPush(T value)
    {
        _ = value;
        return false;
    }
}

/// <summary>Base for typed context pools. Stub.</summary>
public abstract class JsonContextPoolBase<T>
    where T : JsonOperationContext
{
    public abstract IDisposable AllocateOperationContext(out T context);
}

// ---------------------------------------------------------------------------
// Unmanaged buffer wrapper
// ---------------------------------------------------------------------------

/// <summary>Wrapper around a raw unmanaged buffer exposing it as <see cref="System.Memory{T}"/>. Stub.</summary>
public sealed unsafe class UnmanagedMemory
{
    public UnmanagedMemory(byte* address, int size)
    {
        _ = address;
        _ = size;
    }

    public Memory<byte> Memory => throw new NotImplementedException();
}

// ---------------------------------------------------------------------------
// Text writer (async)
// ---------------------------------------------------------------------------

/// <summary>Async streaming blittable JSON text writer. Stub.</summary>
public sealed class AsyncBlittableJsonTextWriter : IAsyncDisposable
{
    public AsyncBlittableJsonTextWriter(JsonOperationContext context, Stream stream)
    {
        _ = context;
        _ = stream;
    }

    public void WriteObject(BlittableJsonReaderObject obj) => throw new NotImplementedException();

    public ValueTask FlushAsync(CancellationToken token = default) => throw new NotImplementedException();

    public ValueTask DisposeAsync() => throw new NotImplementedException();
}

/// <summary>Synchronous blittable JSON text writer. Stub.</summary>
public sealed class BlittableJsonTextWriter : IDisposable
{
    public BlittableJsonTextWriter(JsonOperationContext context, Stream stream)
    {
        _ = context;
        _ = stream;
    }

    public void WriteObject(BlittableJsonReaderObject obj) => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}

/// <summary>Factory for recyclable memory streams. Stub.</summary>
public static class RecyclableMemoryStreamFactory
{
    public static MemoryStream GetRecyclableStream() => new MemoryStream();
}

/// <summary>Vector data header. Stub.</summary>
public readonly struct BlittableVectorHeader
{
    public BlittableVectorType Type => throw new NotImplementedException();
    public int Length => throw new NotImplementedException();
}

/// <summary>Hashing helpers. Stub.</summary>
public static class Hashing
{
    public static ulong Combine(ulong a, ulong b) => throw new NotImplementedException();
    public static ulong Mix(ulong v) => throw new NotImplementedException();

    public static class XXHash64
    {
        public static unsafe ulong Calculate(byte* buffer, ulong length, ulong seed = 0)
            => throw new NotImplementedException();

        public static unsafe ulong Calculate(byte* buffer, int length, ulong seed = 0)
            => throw new NotImplementedException();

        public static ulong Calculate(string value) => throw new NotImplementedException();
    }
}
