using IronDB.Core.Platform;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Server.Utils;

public readonly struct Progressive<TNumber> : IBufferGrowth
    where TNumber : unmanaged, INumber<TNumber>
{
    public int GetInitialSize(in long initialSize)
    {
        long suggested = 4 * Math.Min(Math.Max(Constants.Size.Kilobyte, initialSize), 16 * Constants.Size.Kilobyte);
        long size = Math.Max(suggested, initialSize);

        if (size > IBufferGrowth.MaxBufferSizeInBytes)
        {
            size = IBufferGrowth.MaxBufferSizeInBytes;
        }

        int truncated = (int)size;

        // Represent array as N*sizeof(long)
        return truncated - (truncated % Unsafe.SizeOf<TNumber>());
    }

    public int GetNewSize(in int currentSizeInBytes)
    {
        // Slower growth on 32-bit platforms
        float platformScalar = PlatformDetails.Is32Bits ? 1.1f : 1.5f;

        long size = currentSizeInBytes > 16 * Constants.Size.Megabyte
            ? (long)(currentSizeInBytes * platformScalar)
            : (long)currentSizeInBytes * 2;

        if (size > IBufferGrowth.MaxBufferSizeInBytes)
        {
            size = IBufferGrowth.MaxBufferSizeInBytes;
        }

        int truncated = (int)size;

        // Represent array as N*sizeof(long)
        return truncated - (truncated % Unsafe.SizeOf<TNumber>());
    }

    public bool GrowingThresholdExceed(in int count, in int sizeInBytes)
    {
        // 1/16 left
        var amountOfLongs = sizeInBytes / Unsafe.SizeOf<TNumber>();
        return (amountOfLongs - count) < Math.Max(1, amountOfLongs / 16);
    }


}