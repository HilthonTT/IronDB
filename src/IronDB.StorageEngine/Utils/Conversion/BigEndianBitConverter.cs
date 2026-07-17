namespace IronDB.StorageEngine.Utils.Conversion;

/// <summary>
/// Implementation of EndianBitConverter which converts to/from big-endian
/// byte arrays.
/// </summary>
public sealed class BigEndianBitConverter : EndianBitConverter
{
    public override Endianness Endianness => Endianness.BigEndian;

    public override bool IsLittleEndian()
    {
        return false;
    }

    protected override void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
    {
        int endOffset = index + bytes - 1;
        for (int i = 0; i < bytes; i++)
        {
            buffer[endOffset - i] = unchecked((byte)(value & 0xff));
            value >>= 8;
        }
    }

    protected override long FromBytes(byte[] value, int startIndex, int bytesToConvert)
    {
        long ret = 0;
        for (int i = 0; i < bytesToConvert; i++)
        {
            ret = unchecked((ret << 8) | value[startIndex + i]);
        }
        return ret;
    }

    protected override unsafe long FromBytes(byte* value, int bytesToConvert)
    {
        long ret = 0;
        for (int i = 0; i < bytesToConvert; i++)
        {
            ret = (ret << 8) | value[i];
        }
        return ret;
    }
}
