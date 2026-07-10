using System.Runtime.CompilerServices;

namespace IronDB.Core.Server.Strings;

public readonly struct NoStringDistance : IStringDistance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetDistance(ReadOnlySpan<byte> target, ReadOnlySpan<byte> other)
    {
        return 0f;
    }
}