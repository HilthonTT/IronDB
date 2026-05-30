namespace IronDB.Core.Json;

public enum BlittableVectorType : byte
{
    SByte = 0b0000_0001,  // 1 byte
    Int16 = 0b0000_0010,  // 2 bytes
    Int32 = 0b0000_0100,  // 4 bytes
    Int64 = 0b0000_1000,  // 8 bytes

    Byte = 0b1000_0001,  // 1 byte
    UInt16 = 0b1000_0010,  // 2 bytes
    UInt32 = 0b1000_0100,  // 4 bytes
    UInt64 = 0b1000_1000,  // 8 bytes

    Half = 0b1100_0010, // 2 bytes
    Float = 0b1100_0100, // 4 bytes
    Double = 0b1100_1000, // 8 bytes
}
