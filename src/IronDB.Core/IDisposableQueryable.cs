#if NET6_0_OR_GREATER
#endif

namespace IronDB.Core;

public interface IDisposableQueryable
{
    bool IsDisposed { get; }
}