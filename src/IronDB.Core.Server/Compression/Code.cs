using System.Runtime.InteropServices;

namespace IronDB.Core.Server.Compression;

[StructLayout(LayoutKind.Explicit)]
internal struct Code
{
    [FieldOffset(0)]
    public int Value;

    [FieldOffset(4)]
    public sbyte Length;
}
