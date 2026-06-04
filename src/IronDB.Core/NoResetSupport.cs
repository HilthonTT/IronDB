using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal readonly struct NoResetSupport<T> : IResetSupport<T> where T : class
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(T value) { }
}