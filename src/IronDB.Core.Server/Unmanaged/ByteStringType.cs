namespace IronDB.Core.Server.Unmanaged;

[Flags]
public enum ByteStringType : byte
{
    Immutable = 0x00, // This is a shorthand for an internal-immutable string. 
    Mutable = 0x01,
    External = 0x02,
    Disposed = 0x04,
    Reserved2 = 0x08, // This bit is reserved for future uses.

    // These flags are unused and can be used by users to store custom information on the instance.
    UserDefined1 = 0x10,
    UserDefined2 = 0x20,
    UserDefined3 = 0x40,
    UserDefined4 = 0x80,

    /// <summary>
    /// Use this value to mask out the user defined bits using the (Bitwise AND) operator.
    /// </summary>
    ByteStringMask = 0x0F,

    /// <summary>
    /// Use this value to mask out the ByteStringType bits using the (Bitwise AND) operator.
    /// </summary>
    UserDefinedMask = 0xF0
}