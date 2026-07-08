using System.Buffers;

namespace IronDB.Core.Server.Compression;

public sealed class AdaptiveMemoryEncoderStats : IEncoderState
{
    private byte[] _encodingBuffer = [];
    private byte[] _decodingBuffer = [];
    private int _size;

    public bool CanGrow => true;

    public Span<byte> EncodingTable => _encodingBuffer.AsSpan(0, _size);

    public Span<byte> DecodingTable => _decodingBuffer.AsSpan(0, _size);

    public void Dispose()
    {
        if (_encodingBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_encodingBuffer);
            _encodingBuffer = [];
        }

        if (_decodingBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_decodingBuffer);
            _decodingBuffer = [];
        }
    }

    public void Grow(int minimumSize)
    {
        // PERF: The encoder knows the size of the encoder state before starting and the initial size may not be big enough.
        // The implementation of the encoder will call Grow() if the encoder state signals that it can grow to the desired size.
        // Since the encoder hasnt started yet to work, there is nothing of interest into the encoding & decoding buffers,
        // therefore, there is no need to spend time copying the content at the moment of growth. This could change in the
        // future and would require adjusting the implementation of this method.           

        if (_encodingBuffer.Length < minimumSize)
        {
            ArrayPool<byte>.Shared.Return(_encodingBuffer);
            _encodingBuffer = ArrayPool<byte>.Shared.Rent(minimumSize);
        }

        if (_decodingBuffer.Length < minimumSize)
        {
            ArrayPool<byte>.Shared.Return(_decodingBuffer);
            _decodingBuffer = ArrayPool<byte>.Shared.Rent(minimumSize);
        }

        _size = minimumSize;
    }
}
