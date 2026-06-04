using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal readonly struct ListResetBehavior<T1> : IResetSupport<List<T1>>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IResetSupport<List<T1>>.Reset(List<T1> value)
    {
        value.Clear();
    }
}
