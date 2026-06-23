using IronDB.Core.Collections;
using UsageMode = IronDB.Core.Json.BlittableJsonDocumentBuilder.UsageMode;
using WriteToken = IronDB.Core.Json.BlittableJsonDocumentBuilder.WriteToken;
using PropertyTag = IronDB.Core.Json.AbstractBlittableJsonDocumentBuilder.PropertyTag;

namespace IronDB.Core.Json;

/// <summary>
/// Writes blittable JSON values into an underlying unmanaged write buffer of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// Skeleton only. The members below capture the surface area required by
/// <see cref="BlittableJsonDocumentBuilder"/> and <see cref="ManualBlittableJsonDocumentBuilder{TWriter}"/>;
/// the bodies are not yet implemented.
/// </remarks>
public sealed unsafe class BlittableWriter<T> : IDisposable
    where T : struct, IUnmanagedWriteBuffer
{
    private readonly JsonOperationContext _context;

    public BlittableWriter(JsonOperationContext context)
    {
        _context = context;
    }

    public CachedProperties CachedProperties => _context.CachedProperties!;

    public int Position => throw new NotImplementedException();

    public int SizeInBytes => throw new NotImplementedException();

    public void Reset() => throw new NotImplementedException();

    public void ResetAndRenew() => throw new NotImplementedException();

    public WriteToken WriteObjectMetadata(FastList<PropertyTag> properties, long firstWrite, int maxPropId)
        => throw new NotImplementedException();

    public int WriteArrayMetadata(FastList<int> positions, FastList<BlittableJsonToken> types, ref BlittableJsonToken listToken)
        => throw new NotImplementedException();

    public void WriteDocumentMetadata(int rootOffset, BlittableJsonToken documentToken)
        => throw new NotImplementedException();

    public int WriteValue(bool value) => throw new NotImplementedException();

    public int WriteValue(long value) => throw new NotImplementedException();

    public int WriteValue(ulong value) => throw new NotImplementedException();

    public int WriteValue(float value) => throw new NotImplementedException();

    public int WriteValue(double value) => throw new NotImplementedException();

    public int WriteValue(decimal value) => throw new NotImplementedException();

    public int WriteValue(byte value) => throw new NotImplementedException();

    public int WriteValue(LazyNumberValue value) => throw new NotImplementedException();

    public int WriteValue(string value, out BlittableJsonToken token, UsageMode mode)
        => throw new NotImplementedException();

    public int WriteValue(LazyStringValue value, out BlittableJsonToken token, UsageMode mode, int? initialCompressedSize)
        => throw new NotImplementedException();

    public int WriteValue(LazyCompressedStringValue value, out BlittableJsonToken token, UsageMode mode)
        => throw new NotImplementedException();

    public int WriteValue(byte* buffer, int size) => throw new NotImplementedException();

    public int WriteValue(byte* buffer, int size, out BlittableJsonToken token, UsageMode mode, int? initialCompressedSize)
        => throw new NotImplementedException();

    public int WriteValue(byte* buffer, int size, FastList<int> escapePositions, out BlittableJsonToken token, UsageMode mode, int? initialCompressedSize)
        => throw new NotImplementedException();

    public int WriteNull() => throw new NotImplementedException();

    public int WriteVector<TValue>(ReadOnlySpan<TValue> vector) where TValue : unmanaged
        => throw new NotImplementedException();

    public BlittableJsonReaderObject CreateReader() => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}
