using System.Collections.Concurrent;
using Serilog;

namespace IronDB.BufferManagement;

/// <summary>
/// A manager to handle buffers for the socket connections.
/// </summary>
/// <remarks>
/// When used in an async call a buffer is pinned. Large numbers of pinned buffers
/// cause problems with the GC (in particular it causes heap fragmentation).
/// This class maintains a set of large segments and gives clients pieces of these
/// segments that they can use for their buffers. The alternative to this would be to
/// create many small arrays which it then maintained. This methodology should be slightly
/// better than the many small array methodology because in creating only a few very
/// large objects it will force these objects to be placed on the LOH. Since the
/// objects are on the LOH they are at this time not subject to compacting which would
/// require an update of all GC roots as would be the case with lots of smaller arrays
/// that were in the normal heap.
/// </remarks>
public sealed class BufferManager
{
    private const int TrialsCount = 100;

    private static readonly ILogger Logger = Log.ForContext<BufferManager>();

    private static BufferManager? _defaultBufferManager;

    private readonly int _segmentChunks;
    private readonly int _chunkSize;
    private readonly int _segmentSize;
    private readonly bool _allowedToCreateMemory;

    private readonly ConcurrentStack<ArraySegment<byte>> _buffers = new();
    private readonly List<byte[]> _segments = [];
    private readonly Lock _creatingNewSegmentLock = new();

    /// <summary>
    /// Gets the default buffer manager.
    /// </summary>
    /// <remarks>You should only be using this method if you don't want to manage buffers on your own.</remarks>
    public static BufferManager Default
    {
        get
        {
            // default to 1024 1kb buffers if people don't want to manage it on their own
            _defaultBufferManager ??= new BufferManager(1024, 1024, 1);
            return _defaultBufferManager;
        }
    }

    public static void SetDefaultBufferManager(BufferManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        _defaultBufferManager = manager;
    }

    public int ChunkSize => _chunkSize;

    public int SegmentSize => _segmentSize;

    public int SegmentChunksCount => _segmentChunks;

    /// <summary>The current number of buffers available.</summary>
    public int AvailableBuffers => _buffers.Count;

    /// <summary>The total size of all buffers.</summary>
    public int TotalBufferSize => _segments.Count * _segmentSize;

    /// <summary>Constructs a new <see cref="BufferManager"/>.</summary>
    /// <param name="segmentChunks">The number of chunks to create per segment.</param>
    /// <param name="chunkSize">The size of a chunk in bytes.</param>
    public BufferManager(int segmentChunks, int chunkSize)
        : this(segmentChunks, chunkSize, 1)
    {
    }

    /// <summary>Constructs a new <see cref="BufferManager"/>.</summary>
    /// <param name="segmentChunks">The number of chunks to create per segment.</param>
    /// <param name="chunkSize">The size of a chunk in bytes.</param>
    /// <param name="initialSegments">The initial number of segments to create.</param>
    public BufferManager(int segmentChunks, int chunkSize, int initialSegments)
        : this(segmentChunks, chunkSize, initialSegments, true)
    {
    }

    /// <summary>Constructs a new <see cref="BufferManager"/>.</summary>
    /// <param name="segmentChunks">The number of chunks to create per segment.</param>
    /// <param name="chunkSize">The size of a chunk in bytes.</param>
    /// <param name="initialSegments">The initial number of segments to create.</param>
    /// <param name="allowedToCreateMemory">If false, when empty and check-out is called an exception will be thrown.</param>
    public BufferManager(int segmentChunks, int chunkSize, int initialSegments, bool allowedToCreateMemory)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(segmentChunks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSegments);

        _segmentChunks = segmentChunks;
        _chunkSize = chunkSize;
        _segmentSize = _segmentChunks * _chunkSize;

        // Allow creation during the initial seeding regardless of the caller's preference; we
        // honour the caller's preference for all subsequent allocations.
        _allowedToCreateMemory = true;
        for (int i = 0; i < initialSegments; i++)
        {
            CreateNewSegment(forceCreation: true);
        }
        _allowedToCreateMemory = allowedToCreateMemory;
    }

    /// <summary>Creates a new segment and makes its buffers available.</summary>
    private void CreateNewSegment(bool forceCreation)
    {
        if (!_allowedToCreateMemory)
        {
            throw new UnableToCreateMemoryException();
        }

        lock (_creatingNewSegmentLock)
        {
            if (!forceCreation && _buffers.Count > _segmentChunks / 2)
            {
                return;
            }

            var bytes = new byte[_segmentSize];
            _segments.Add(bytes);

            for (int i = 0; i < _segmentChunks; i++)
            {
                _buffers.Push(new ArraySegment<byte>(bytes, i * _chunkSize, _chunkSize));
            }

            Logger.Debug(
                "Segments count: {segments}, buffers count: {buffers}, should be when full: {full}",
                _segments.Count, _buffers.Count, _segments.Count * _segmentChunks);
        }
    }

    /// <summary>Checks out a buffer from the manager.</summary>
    /// <remarks>
    /// It is the client's responsibility to return the buffer to the manager by calling
    /// <see cref="CheckIn(ArraySegment{byte})"/> on the buffer.
    /// </remarks>
    public ArraySegment<byte> CheckOut()
    {
        for (int trial = 0; trial < TrialsCount; trial++)
        {
            if (_buffers.TryPop(out ArraySegment<byte> result))
            {
                return result;
            }
            CreateNewSegment(forceCreation: false);
        }

        throw new UnableToAllocateBufferException();
    }

    /// <summary>Checks out a number of buffers from the manager.</summary>
    /// <remarks>
    /// It is the client's responsibility to return the buffers to the manager by calling
    /// <see cref="CheckIn(IEnumerable{ArraySegment{byte}})"/> on the result.
    /// </remarks>
    public IEnumerable<ArraySegment<byte>> CheckOut(int toGet)
    {
        var result = new ArraySegment<byte>[toGet];
        int totalReceived = 0;

        try
        {
            for (int trial = 0; trial < TrialsCount; trial++)
            {
                while (totalReceived < toGet)
                {
                    if (!_buffers.TryPop(out ArraySegment<byte> piece))
                    {
                        break;
                    }
                    result[totalReceived++] = piece;
                }

                if (totalReceived == toGet)
                {
                    return result;
                }
                CreateNewSegment(forceCreation: false);
            }

            throw new UnableToAllocateBufferException();
        }
        catch (Exception)
        {
            if (totalReceived > 0)
            {
                CheckIn(result.Take(totalReceived));
            }
            throw;
        }
    }

    /// <summary>Returns a buffer to the control of the manager.</summary>
    public void CheckIn(ArraySegment<byte> buffer)
    {
        CheckBuffer(buffer);
        _buffers.Push(buffer);
    }

    /// <summary>Returns a set of buffers to the control of the manager.</summary>
    public void CheckIn(IEnumerable<ArraySegment<byte>> buffersToReturn)
    {
        ArgumentNullException.ThrowIfNull(buffersToReturn);

        foreach (ArraySegment<byte> buf in buffersToReturn)
        {
            CheckBuffer(buf);
            _buffers.Push(buf);
        }
    }

    private void CheckBuffer(ArraySegment<byte> buffer)
    {
        if (buffer.Array is null || buffer.Count == 0 || buffer.Array.Length < buffer.Offset + buffer.Count)
        {
            throw new InvalidBufferException("Attempt to check in an invalid buffer.");
        }
        if (buffer.Count != _chunkSize)
        {
            throw new ArgumentException("Buffer was not of the same chunk size as the buffer manager.", nameof(buffer));
        }
    }
}
