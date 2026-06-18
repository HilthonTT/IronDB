using IronDB.Core.Json.Parsing;

namespace IronDB.Core.Json;

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

/// <summary>Base for typed context pools. Stub.</summary>
public abstract class JsonContextPoolBase<T>
    where T : JsonOperationContext
{
    public abstract IDisposable AllocateOperationContext(out T context);
}

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
