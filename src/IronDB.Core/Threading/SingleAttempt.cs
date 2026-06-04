namespace IronDB.Core.Threading;

public struct SingleAttempt : IDisposeOnceOperationMode
{
    private long _diposeDepth;

    public bool DuringDispose => Interlocked.Read(ref _diposeDepth) != 0;

    public void EnterDispose()
    {
        Interlocked.Increment(ref _diposeDepth);
    }

    public void Initialize()
    {
        _diposeDepth = 0;
    }

    public void LeaveDispose()
    {
        Interlocked.Decrement(ref _diposeDepth);
    }
}
