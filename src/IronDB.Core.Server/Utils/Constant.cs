using System.Numerics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Server.Utils;

public readonly struct Constant<TNumber> : IBufferGrowth
    where TNumber : unmanaged, INumber<TNumber>
{
    public int GetInitialSize(in long initialSize)
    {
        return (int) initialSize *Unsafe.SizeOf<TNumber>();
    }

    public int GetNewSize(in int currentSizeInBytes) => currentSizeInBytes * 2;
    public bool GrowingThresholdExceed(in int count, in int sizeInBytes)
    {
        var amountOfLongs = (sizeInBytes / Unsafe.SizeOf<TNumber>());
        return (amountOfLongs - count) < amountOfLongs / 16;
    }
}
