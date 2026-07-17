namespace IronDB.StorageEngine.Utils.Conversion;

/// <summary>
/// Implementation of EndianBitConverter which converts to/from little-endian
/// byte arrays.
/// </summary>
public sealed class LittleEndianBitConverter : EndianBitConverter
{
    public override Endianness Endianness => Endianness.LittleEndian;

    public override bool IsLittleEndian()
    {
        return true;
    }

    protected override void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
    {
        for (int i = 0; i < bytes; i++)
        {
            buffer[i + index] = unchecked((byte)(value & 0xff));
            value >>= 8;
        }
    }

    protected override long FromBytes(byte[] value, int startIndex, int bytesToConvert)
    {
        long ret = 0;
        for (int i = 0; i < bytesToConvert; i++)
        {
            ret = unchecked((ret << 8) | value[startIndex + bytesToConvert - 1 - i]);
        }
        return ret;
    }

    protected override unsafe long FromBytes(byte* value, int bytesToConvert)
    {
        long ret = 0;
        for (int i = 0; i < bytesToConvert; i++)
        {
            ret = unchecked((ret << 8) | value[bytesToConvert - 1 - i]);
        }
        return ret;
    }
}
