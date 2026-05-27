using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal static class Branchless
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int ToInt32(this bool value)
    {
        return *(byte*)&value;
    }
}
