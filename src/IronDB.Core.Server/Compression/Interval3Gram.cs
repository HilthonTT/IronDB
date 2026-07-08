using System.Runtime.InteropServices;

namespace IronDB.Core.Server.Compression;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct Interval3Gram
{
    [FieldOffset(0)]
    public uint BufferAndLength;

    [FieldOffset(0)]
    public fixed byte KeyBuffer[3];
    [FieldOffset(3)]
    public byte _prefixAndKeyLength;

    [FieldOffset(4)]
    public Code Code;

    public byte PrefixLength
    {
        readonly get { return (byte)(_prefixAndKeyLength & 0x0F); }
        set { _prefixAndKeyLength = (byte)(_prefixAndKeyLength & 0xF0 | value & 0x0F); }
    }

    public byte KeyLength
    {
        readonly get { return (byte)(_prefixAndKeyLength >> 4); }
        set { _prefixAndKeyLength = (byte)(_prefixAndKeyLength & 0x0F | (value << 4)); }
    }
}