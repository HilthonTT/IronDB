using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal readonly struct DictionaryResetBehavior<T1, T2> : IResetSupport<Dictionary<T1, T2>>
    where T1 : notnull
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IResetSupport<Dictionary<T1, T2>>.Reset(Dictionary<T1, T2> value)
    {
        value.Clear();
    }
}