using System.Runtime.InteropServices;

namespace IronDB.Core.Json;

public abstract unsafe class AbstractBlittableJsonTextWriter
{
    private static ReadOnlySpan<byte> _controlCodeEscapes => 
        "0000000100020003000400050006000700080009000A000B000C000D000E000F0010001100120013001400150016001700180019001A001B001C001D001E001F"u8;

    internal static ReadOnlySpan<int> ControlCodeEscapes => MemoryMarshal.Cast<byte, int>(_controlCodeEscapes);
}
