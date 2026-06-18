using System.Runtime.InteropServices;

namespace IronDB.Core.Json;

/// <summary>Vector data header.</summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
internal struct BlittableVectorHeader(BlittableVectorType type, int count)
{
    private const byte TypeMask = 0b0011_1111;
    private const byte FloatFlag = 0b0100_0000;
    private const byte UnsignedFlag = 0b1000_0000;

    [FieldOffset(0)]
    private readonly BlittableVectorType _type = type;

    [FieldOffset(1)]
    public byte AlignmentOffset;

    [FieldOffset(2)]
    public int Count = count;

    public readonly BlittableVectorType Type => _type;

    public readonly bool IsUnsigned => (BlittableVectorType)((byte)_type & UnsignedFlag) != 0;

    public readonly bool IsFloatingPoint => (BlittableVectorType)((byte)_type & FloatFlag) != 0;

    public readonly int ElementSize => ((byte)Type) & TypeMask;
}
