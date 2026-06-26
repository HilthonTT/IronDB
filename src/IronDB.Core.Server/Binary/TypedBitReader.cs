using System.Runtime.CompilerServices;

namespace IronDB.Core.Server.Binary;

public struct TypedBitReader<T> : IBitReader
    where T : unmanaged
{
    private int _length;
    private int _shift;
    private readonly T _data;

    public TypedBitReader(T data)
    {
        _length = Unsafe.SizeOf<T>() * 8;
        _data = data;
        _shift = 0;
    }

    public TypedBitReader(T data, int length, int skipped = 0)
    {
        _length = length;
        _data = data;
        _shift = 0;

        if (skipped != 0)
        {
            Skip(skipped);
        }
    }

    public readonly int Length => _length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bit Read()
    {
        if (_length == 0)
        {
            throw new InvalidOperationException("Cannot read from a 0 length stream");
        }

        byte value;
        if (typeof(long) == typeof(T))
        {
            value = (byte)(((ulong)(long)(object)_data) >> (sizeof(ulong) * 8 - 1 - _shift));
        }
        else if (typeof(ulong) == typeof(T))
        {
            value = (byte)(((ulong)(object)_data) >> (sizeof(ulong) * 8 - 1 - _shift));
        }
        else if (typeof(int) == typeof(T))
        {
            value = (byte)(((uint)(int)(object)_data) >> (sizeof(uint) * 8 - 1 - _shift));
        }
        else if (typeof(uint) == typeof(T))
        {
            value = (byte)(((uint)(object)_data) >> (sizeof(uint) * 8 - 1 - _shift));
        }
        else if (typeof(short) == typeof(T))
        {
            value = (byte)(((ushort)(short)(object)_data) >> (sizeof(ushort) * 8 - 1 - _shift));
        }
        else if (typeof(ushort) == typeof(T))
        {
            value = (byte)(((ushort)(object)_data) >> (sizeof(ushort) * 8 - 1 - _shift));
        }
        else if (typeof(sbyte) == typeof(T))
        {
            value = (byte)(((byte)(sbyte)(object)_data) >> (sizeof(byte) * 8 - 1 - _shift));
        }
        else if (typeof(byte) == typeof(T))
        {
            value = (byte)(((byte)(object)_data) >> (sizeof(byte) * 8 - 1 - _shift));
        }
        else
        {
            throw new ArgumentException($"Type '{nameof(T)}' is not supported by this reader.");
        }

        _shift++;
        _length--;
        return new Bit(value);
    }

    public void Skip(int bits)
    {
        _shift = (byte)(_shift + bits);
    }
}
