namespace IronDB.Core.Threading;

public interface IDisposeOnceOperationMode
{
    void Initialize();

    bool DuringDispose { get; }

    void EnterDispose();

    void LeaveDispose();
}