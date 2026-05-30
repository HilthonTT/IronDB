// STUBS: minimal types added to satisfy the compiler while the full port is in progress.
// All members throw NotImplementedException — do not rely on runtime behavior.

namespace IronDB.Core.Threading;

/// <summary>Strategy marker for <see cref="DisposeOnce{T}"/>. Stub.</summary>
public sealed class SingleAttempt
{
}

/// <summary>Runs a dispose action at most once. Stub — no implementation.</summary>
public sealed class DisposeOnce<TStrategy>
{
    public DisposeOnce(Action action)
    {
        _ = action;
    }

    public bool Disposed => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}
