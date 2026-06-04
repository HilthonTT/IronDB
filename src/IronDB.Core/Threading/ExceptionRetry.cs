namespace IronDB.Core.Threading;

internal readonly struct ExceptionRetry : IDisposeOnceOperationMode
{
    public readonly bool DuringDispose => false;

    public void EnterDispose()
    {
    }

    public void Initialize()
    {
    }

    public void LeaveDispose()
    {
    }
}