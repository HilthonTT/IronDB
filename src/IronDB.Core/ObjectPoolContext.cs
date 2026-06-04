using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal readonly struct ObjectPoolContext<T, TR> : IDisposable
    where T : class
    where TR : struct, IResetSupport<T>
{
    private readonly ObjectPool<T, TR> _owner;
    public readonly T Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ObjectPoolContext(ObjectPool<T, TR> owner, T value)
    {
        _owner = owner;
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        _owner.Free(Value);
    }
}