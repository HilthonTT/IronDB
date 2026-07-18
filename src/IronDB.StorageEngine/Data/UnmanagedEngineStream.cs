using IronDB.Core;

namespace IronDB.StorageEngine.Data;

/// <summary>
/// A read-only stream over an unmanaged (byte*) buffer.
/// Used for inline streams stored directly in tree nodes.
/// The pointer must remain valid for the lifetime of this stream (i.e., the current transaction).
/// </summary>
public sealed unsafe class UnmanagedEngineStream(byte* ptr, int length) : Stream
{
    private byte* _ptr = ptr;
    private readonly int _length = length;
    private int _position = 0;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public sealed override long Length => _length;


    public override long Position
    {
        get => _position;
        set => _position = (int)Math.Clamp(value, 0, _length);
    }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

        if (offset < 0 || offset > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        if (count < 0 || count > buffer.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        int remaining = _length - _position;
        if (remaining <= 0)
        {
            return 0;
        }

        int toRead = Math.Min(count, remaining);
        fixed (byte* dst = buffer)
        {
            Memory.Copy(dst + offset, _ptr + _position, toRead);
        }
        _position += toRead;

        return toRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = _length + offset;
                break;
        }
        return Position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override int ReadByte()
    {
        if (_position >= _length)
        {
            return -1;
        }
        return _ptr[_position++];
    }

    /// <summary>
    /// Updates the pointer to point to a new transaction's page memory.
    /// The length must remain the same.
    /// </summary>
    public void UpdatePtr(byte* ptr)
    {
        _ptr = ptr;
    }
}
