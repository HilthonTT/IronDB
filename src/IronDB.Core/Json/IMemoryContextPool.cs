namespace IronDB.Core.Json;

public interface IMemoryContextPool : IDisposable
{
    IDisposable AllocateOperationContext(out JsonOperationContext context);
}
