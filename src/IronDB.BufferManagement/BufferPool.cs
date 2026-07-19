namespace IronDB.BufferManagement;

/// <summary>
/// A growable byte buffer backed by chunks obtained from a <see cref="BufferManager"/>.
/// </summary>
public class BufferPool : IDisposable
{
    private readonly BufferManager _bufferManager;
    private readonly int _chunkSize;

    private List<ArraySegment<byte>> _buffers;
    private int _length;
    private bool _disposed;

    /// <summary>Gets the capacity of the <see cref="BufferPool"/> in bytes.</summary>
    public int Capacity
    {
        get
        {
            CheckDisposed();
            return _chunkSize * _buffers.Count;
        }
    }

    /// <summary>Gets the current length of the <see cref="BufferPool"/> in bytes.</summary>
    public int Length => _length;

    /// <summary>Gets the populated buffers contained in this <see cref="BufferPool"/>.</summary>
    public IEnumerable<ArraySegment<byte>> EffectiveBuffers
    {
        get
        {
            CheckDisposed();
            if (_length == 0)
            {
                yield break;
            }

            Position l = GetPositionFor(_length);
            // full buffers
            for (int i = 0; i < l.Index; i++)
            {
                yield return _buffers[i];
            }

            // partial trailing buffer
            ArraySegment<byte> last = _buffers[l.Index];
            yield return new ArraySegment<byte>(last.Array ?? [], last.Offset, l.Offset);
        }
    }

    /// <summary>Gets or sets the byte at the specified index.</summary>
    public byte this[int index]
    {
        get
        {
            CheckDisposed();
            if (index < 0 || index >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            Position l = GetPositionFor(index);
            ArraySegment<byte> buffer = _buffers[l.Index];
            return buffer.Array is null ? (byte)0 : buffer.Array[buffer.Offset + l.Offset];
        }
        set
        {
            CheckDisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            Position l = GetPositionFor(index);
            EnsureCapacity(l);
            ArraySegment<byte> buffer = _buffers[l.Index];
            if (buffer.Array is not null)
            {
                buffer.Array[buffer.Offset + l.Offset] = value;
            }
            if (_length <= index)
            {
                _length = index + 1;
            }
        }
    }

    /// <summary>Initializes a new instance backed by the default <see cref="BufferManager"/>.</summary>
    public BufferPool()
        : this(1, BufferManager.Default)
    {
    }

    /// <summary>Initializes a new instance backed by the given <paramref name="bufferManager"/>.</summary>
    public BufferPool(BufferManager bufferManager)
        : this(1, bufferManager)
    {
    }

    /// <summary>Initializes a new instance.</summary>
    /// <param name="initialBufferCount">The number of initial buffers.</param>
    /// <param name="bufferManager">The buffer manager.</param>
    public BufferPool(int initialBufferCount, BufferManager bufferManager)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialBufferCount);
        ArgumentNullException.ThrowIfNull(bufferManager);

        _length = 0;
        _buffers = [.. bufferManager.CheckOut(initialBufferCount)];
        // must have at least 1 buffer
        _chunkSize = _buffers[0].Count;
        _bufferManager = bufferManager;
        _disposed = false;
    }

    /// <summary>Appends the specified data to the end of the pool.</summary>
    public void Append(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Write(_length, data, 0, data.Length);
    }

    /// <summary>Appends a range of the specified data to the end of the pool.</summary>
    public void Append(byte[] data, int offset, int count)
        => Write(_length, data, offset, count);

    /// <summary>Writes data starting at the given position.</summary>
    public void Write(int position, byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (offset < 0 || offset > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        if (count < 0 || count + offset > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        Write(position, new ArraySegment<byte>(data, offset, count));
    }

    /// <summary>Writes data starting at the given position.</summary>
    public void Write(int position, ArraySegment<byte> data)
    {
        CheckDisposed();
        int written = 0;
        int tmpLength = position;

        do
        {
            Position loc = GetPositionFor(tmpLength);
            EnsureCapacity(loc);

            ArraySegment<byte> current = _buffers[loc.Index];
            int canWrite = data.Count - written;
            int available = current.Count - loc.Offset;
            canWrite = canWrite > available ? available : canWrite;
            if (canWrite > 0 && data.Array is not null && current.Array is not null)
            {
                Buffer.BlockCopy(
                    data.Array, written + data.Offset,
                    current.Array, current.Offset + loc.Offset,
                    canWrite);
            }
            written += canWrite;
            tmpLength += canWrite;
        }
        while (written < data.Count);

        _length = tmpLength > _length ? tmpLength : _length;
    }

    /// <summary>Reads data starting at the given position.</summary>
    public int ReadFrom(int position, byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (offset < 0 || offset > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        if (count < 0 || count + offset > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        return ReadFrom(position, new ArraySegment<byte>(data, offset, count));
    }

    /// <summary>Reads data starting at the given position.</summary>
    public int ReadFrom(int position, ArraySegment<byte> data)
    {
        CheckDisposed();
        if (position >= _length)
        {
            return 0;
        }

        int copied = 0;
        int left = Math.Min(data.Count, _length - position);
        int currentLocation = position;
        while (left > 0)
        {
            Position l = GetPositionFor(currentLocation);
            ArraySegment<byte> current = _buffers[l.Index];

            if (current.Array is null || data.Array is null)
            {
                return 0;
            }

            int bytesToRead = Math.Min(_chunkSize - l.Offset, left);
            if (bytesToRead > 0)
            {
                Buffer.BlockCopy(
                    current.Array, current.Offset + l.Offset,
                    data.Array, data.Offset + copied,
                    bytesToRead);
                copied += bytesToRead;
                left -= bytesToRead;
                currentLocation += bytesToRead;
            }
        }

        return copied;
    }

    /// <summary>Sets the length of the <see cref="BufferPool"/>.</summary>
    public void SetLength(int newLength) => SetLength(newLength, releaseMemory: true);

    /// <summary>Sets the length of the <see cref="BufferPool"/>.</summary>
    /// <param name="newLength">The new length.</param>
    /// <param name="releaseMemory">If true, any memory no longer used is released.</param>
    public void SetLength(int newLength, bool releaseMemory)
    {
        CheckDisposed();
        ArgumentOutOfRangeException.ThrowIfNegative(newLength);
        int oldCapacity = Capacity;
        _length = newLength;

        if (_length < oldCapacity && releaseMemory)
        {
            RemoveCapacity(GetPositionFor(_length));
        }
        else if (_length > oldCapacity)
        {
            EnsureCapacity(GetPositionFor(_length));
        }
    }

    private void RemoveCapacity(Position position)
    {
        while (_buffers.Count > position.Index + 1)
        {
            _bufferManager.CheckIn(_buffers[^1]);
            _buffers.RemoveAt(_buffers.Count - 1);
        }
    }

    private void EnsureCapacity(Position position)
    {
        if (position.Index < _buffers.Count)
        {
            return;
        }
        foreach (ArraySegment<byte> buffer in _bufferManager.CheckOut(position.Index + 1 - _buffers.Count))
        {
            if (buffer.Count != _chunkSize)
            {
                throw new InvalidBufferException(
                    $"BufferManager returned a buffer of size {buffer.Count}; expected {_chunkSize}.");
            }
            _buffers.Add(buffer);
        }
    }

    /// <summary>Converts this <see cref="BufferPool"/> to a byte array.</summary>
    public byte[] ToByteArray()
    {
        CheckDisposed();
        Position l = GetPositionFor(_length);
        var result = new byte[_length];
        for (int i = 0; i < l.Index; i++)
        {
            ArraySegment<byte> current = _buffers[i];
            if (current.Array is null)
            {
                continue;
            }
            // copy full buffer
            Buffer.BlockCopy(current.Array, current.Offset, result, i * _chunkSize, _chunkSize);
        }

        // copy last partial buffer
        if (l.Index < _buffers.Count)
        {
            ArraySegment<byte> last = _buffers[l.Index];
            if (last.Array is not null)
            {
                Buffer.BlockCopy(last.Array, last.Offset, result, l.Index * _chunkSize, l.Offset);
            }
        }

        return result;
    }

    private Position GetPositionFor(int index)
        => new(index / _chunkSize, index % _chunkSize);

    /// <summary>Returns any memory used by this <see cref="BufferPool"/> to the <see cref="BufferManager"/>.</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing && _buffers.Count > 0)
        {
            _bufferManager.CheckIn(_buffers);
            _buffers = [];
        }

        _disposed = true;
    }

    private void CheckDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private readonly struct Position(int index, int offset)
    {
        public int Index { get; } = index;
        public int Offset { get; } = offset;
    }
}
