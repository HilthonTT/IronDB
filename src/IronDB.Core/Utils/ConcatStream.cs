using System.Buffers;

namespace IronDB.Core.Utils;

internal sealed class ConcatStream : Stream
{
    private readonly RentedBuffer _prefix;
    private readonly Stream? _remaining;
    private bool _disposed;

    public ConcatStream(RentedBuffer prefix, Stream remaining)
    {
        _prefix = prefix;
        _remaining = remaining;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position 
    { 
        get => throw new NotSupportedException(); 
        set => throw new NotSupportedException(); 
    }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, $"{nameof(_remaining)} stream was already disposed.");

        if (_prefix.Count <= 0)
        {
            return _remaining?.Read(buffer, offset, count) ?? 0;
        }

        int read = ReadFromBuffer(buffer, offset, count);
        return read;
    }

    public override async Task<int> ReadAsync(
        byte[] buffer, 
        int offset, 
        int count, 
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, $"{nameof(_remaining)} stream was already disposed.");

        if (_prefix.Count <= 0)
        {
            if (_remaining is null)
            {
                return 0;
            }

            return await _remaining.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        int read = ReadFromBuffer(buffer, offset, count);
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    private int ReadFromBuffer(byte[] buffer, int offset, int count)
    {
        int read = Math.Min(_prefix.Count, count);

        _prefix.Buffer ??= [];
        Buffer.BlockCopy(_prefix.Buffer, _prefix.Offset, buffer, offset, read);
        _prefix.Count -= read;
        _prefix.Offset += read;

        if (_prefix.Count == 0)
        {
            ArrayPool<byte>.Shared.Return(_prefix.Buffer);
            _prefix.Buffer = null;
        }

        return read;
    }

    protected override void Dispose(bool disposing)
    {
        _disposed = true;

        if (_prefix.Buffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_prefix.Buffer);
            _prefix.Buffer = null;
        }

        _remaining?.Dispose();

        base.Dispose(disposing);
    }

    internal sealed class RentedBuffer
    {
        public byte[]? Buffer { get; set; }

        public int Offset { get; set; }

        public int Count { get; set; }
    }
}
