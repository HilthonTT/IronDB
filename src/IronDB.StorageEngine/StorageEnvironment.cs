using IronDB.Core.Server.Unmanaged;
using IronDB.Core.Threading;
using IronDB.StorageEngine.Impl.FreeSpace;
using IronDB.StorageEngine.Utils;

namespace IronDB.StorageEngine;

public sealed class StorageEnvironment : IDisposable
{
    private readonly IFreeSpaceHandling _freeSpaceHandling = new FreeSpaceHandling();

    /// <summary>
    /// This is the shared storage where we are going to store all the static constants for names.
    /// WARNING: This context will never be released, so only static constants should be added here.
    /// </summary>
    private static readonly ByteStringContext _staticContext = new(SharedMultipleUseFlag.None, ByteStringContext.MinBlockSizeInBytes);

    public StorageEnvironmentOptions Options => throw new NotImplementedException();

    public IFreeSpaceHandling FreeSpaceHandling => _freeSpaceHandling;

    public static IDisposable GetStaticContext(out ByteStringContext ctx)
    {
        Monitor.Enter(_staticContext);

        ctx = _staticContext;

        return new DisposableAction(() => Monitor.Exit(_staticContext));
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}