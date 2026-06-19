using System.Buffers;
using System.Runtime.CompilerServices;
using static IronDB.Core.Json.JsonOperationContext;

namespace IronDB.Core.Json.Sync;

internal static class JsonOperationSyncExtensions
{
#if DEBUG
    private static readonly ConditionalWeakTable<Stream, Stream> SeenWithDifferentBuffer = [];
#endif

    public static BlittableJsonReaderObject ReadForMemory(
        this SyncJsonOperationContext context,
        string jsonString,
        string documentId)
    {
        // TODO: Maybe use ManagedPinnedBuffer here
        int maxByteSize = Encodings.Utf8.GetMaxByteCount(jsonString.Length);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxByteSize);
        try
        {
            // PERF: There is no advantage to fix the array, since internally the same will happen.
            // If the framework does indeed improves the implementation to work natively, we will
            // miss it because of it. 
            // https://issues.hibernatingrhinos.com/issue/RavenDB-20321
            Encodings.Utf8.GetBytes(jsonString.AsSpan(), buffer);
            using var ms = new MemoryStream(buffer);
            return ReadForMemory(context, ms, documentId);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static BlittableJsonReaderObject ReadForMemory(
        SyncJsonOperationContext syncContext,
        Stream stream, 
        string documentId)
    {
        return ParseToMemory(syncContext, stream, documentId, BlittableJsonDocumentBuilder.UsageMode.None);
    }

    internal static BlittableJsonReaderObject ParseToMemory(
        SyncJsonOperationContext syncContext,
        Stream stream,
        string debugTag, 
        BlittableJsonDocumentBuilder.UsageMode mode,
        IBlittableDocumentModifier? modifier = null)
    {
#if DEBUG
        if (SeenWithDifferentBuffer.TryGetValue(stream, out _))
        {
            throw new InvalidOperationException("BUG: Stream was already called to ParseToMemory - see RavenDB-18307 - you will corrupt data in this manner.");
        }

        SeenWithDifferentBuffer.Add(stream, stream);
#endif

        using (syncContext.Context.GetMemoryBuffer(out var bytes))
        {
            return ParseToMemory(syncContext, stream, debugTag, mode, bytes, modifier);
        }
    }

    public static BlittableJsonReaderObject ParseToMemory(
        SyncJsonOperationContext syncContext,
        Stream stream,
        string debugTag,
        BlittableJsonDocumentBuilder.UsageMode mode,
        MemoryBuffer bytes,
        IBlittableDocumentModifier? modifier = null)
    {
        syncContext.EnsureNotDisposed();

        syncContext.JsonParserState.Reset();
        using var parser = new UnmanagedJsonParser(syncContext.Context, syncContext.JsonParserState, debugTag);
        using var builder = new BlittableJsonDocumentBuilder(syncContext.Context, mode, debugTag, parser, syncContext.JsonParserState, modifier: modifier);
        syncContext.Context.CachedProperties?.NewDocument();
        builder.ReadObjectDocument();
        while (true)
        {
            if (bytes.Valid == bytes.Used)
            {
                var read = stream.Read(bytes.Memory.Memory.Span);
                syncContext.EnsureNotDisposed();
                if (read == 0)
                {
                    throw new EndOfStreamException("Stream ended without reaching end of json content");
                }
                bytes.Valid = read;
                bytes.Used = 0;
            }
            parser.SetBuffer(bytes);
            var result = builder.Read();
            bytes.Used += parser.BufferOffset;
            if (result)
            {
                break;
            }
        }
        builder.FinalizeDocument();

        var reader = builder.CreateReader();
        return reader;
    }
}
