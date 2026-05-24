namespace IronDB.BufferManagement;

public sealed class BufferPoolStream : Stream
{
    private readonly BufferPool _bufferPool;
    private long _position;

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _bufferPool.Length;

    public override long Position
    {
        get => _position;
        set
        {
            if (value < 0 || value > _bufferPool.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _position = value;
        }
    }

    /// <summary>Initializes a new instance backed by <paramref name="bufferPool"/>.</summary>
    public BufferPoolStream(BufferPool bufferPool)
    {
        ArgumentNullException.ThrowIfNull(bufferPool);
        _bufferPool = bufferPool;
    }

    public override void Flush()
    {
        // no-op
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_position >= _bufferPool.Length)
        {
            return 0;
        }

        int ret = _bufferPool.ReadFrom((int)Position, buffer, offset, count);
        _position += ret;
        return ret;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _bufferPool.Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Unknown SeekOrigin."),
        };
        return Position;
    }

    public override void SetLength(long value)
    {
        _bufferPool.SetLength((int)value);
        if (_position > value)
        {
            _position = value;
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _bufferPool.Write((int)_position, buffer, offset, count);
        _position += count;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _bufferPool.Dispose();
        }
        base.Dispose(disposing);
    }

    public byte[] ToArray() => _bufferPool.ToByteArray();
}
