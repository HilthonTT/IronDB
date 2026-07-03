using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IronDB.Core.Server.Unmanaged;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ByteStringStorage
{
    /// <summary>
    /// The actual type for the byte string
    /// </summary>
    public ByteStringType Flags;

    /// <summary>
    /// The actual length of the byte string
    /// </summary>
    public int Length;

    /// <summary>
    /// This is the pointer to the start of the byte stream. 
    /// </summary>
    public byte* Ptr;

    /// <summary>
    /// This is the total storage size for this byte string. Length will always be smaller than Size - 1.
    /// </summary>
    public int Size;

#if VALIDATE
        public const ulong NullKey = unchecked((ulong)-1);

        /// <summary>
        /// The validation key for the storage value.
        /// </summary>
        public ulong Key;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ulong GetContentHash()
    {
        // Given how the size of slices can vary it is better to lose a bit (10%) on smaller slices 
        // (less than 20 bytes) and to win big on the bigger ones. 
        //
        // After 24 bytes the gain is 10%
        // After 64 bytes the gain is 2x
        // After 128 bytes the gain is 4x.
        //
        // We should control the distribution of this over time.

        // JIT will remove the corresponding line based on the target architecture using dead code removal.
        if (IntPtr.Size == 4)
        {
            return Hashing.XXHash32.CalculateInline(Ptr, Length);
        }

        return Hashing.XXHash64.CalculateInline(Ptr, (ulong)Length);
    }


}
