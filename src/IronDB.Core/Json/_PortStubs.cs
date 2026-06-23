using IronDB.Core.Json.Parsing;

namespace IronDB.Core.Json;

/// <summary>Hook invoked while building a blittable document to mutate it mid-parse. Stub.</summary>
public interface IBlittableDocumentModifier
{
    void StartObject();

    void EndObject();

    void Reset(JsonOperationContext context);

    bool AboutToReadPropertyName(IJsonParser reader, JsonParserState state);
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

    public void ValidateFloat() => throw new NotImplementedException();

    public void ValidatePool() => throw new NotImplementedException();

    public string GenerateErrorState() => throw new NotImplementedException();

    public IJsonParser.OnStringReadDelegate OnStringRead
    {
        set => throw new NotImplementedException();
    }

    public void Dispose() => throw new NotImplementedException();
}

/// <summary>Base for typed context pools. Stub.</summary>
public abstract class JsonContextPoolBase<T>
    where T : JsonOperationContext
{
    public abstract IDisposable AllocateOperationContext(out T context);
}
