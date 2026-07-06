namespace IronDB.Core.Json.Sync;

/// <summary>Synchronous blittable JSON text writer.</summary>
public sealed class BlittableJsonTextWriter(JsonOperationContext context, Stream stream) 
    : AbstractBlittableJsonTextWriter(context, stream),  IDisposable
{
    public void Dispose()
    {
        DisposeInternal();
    }

    public void Flush()
    {
        FlushInternal();
    }
}
