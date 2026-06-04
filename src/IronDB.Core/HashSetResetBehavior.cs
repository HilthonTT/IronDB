using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal readonly struct HashSetResetBehavior<T1> : IResetSupport<HashSet<T1>>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IResetSupport<HashSet<T1>>.Reset(HashSet<T1> value)
    {
        value.Clear();
    }
}