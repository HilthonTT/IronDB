namespace IronDB.Core.Server.Compression;

public readonly unsafe struct NativeMemoryEncoderState(byte* buffer, int _size) : IEncoderState
{
    private readonly byte* _buffer = buffer;
    private readonly int _size = _size;

    public bool CanGrow => false;

    public Span<byte> EncodingTable => new(_buffer, _size / 2);

    public Span<byte> DecodingTable => new(_buffer + _size / 2, _size / 2);

    public void Grow(int minimumSize)
    {
        throw new NotSupportedException($"{nameof(NativeMemoryEncoderState)} does not support '.{nameof(Grow)}()'.");
    }

    public void Dispose()
    {
    }
}
