using IronDB.Core.Server.Unmanaged;

namespace IronDB.StorageEngine.Utils.PFor;

public unsafe struct FastPForBufferedReader : IDisposable
{
    private long* _buffer;
    private int _bufferIdx;
    private int _usedBuffer;

    private readonly ByteStringContext _allocator;
    public FastPForDecoder Decoder;
    private ByteStringContext<ByteStringMemoryCache>.InternalScope _bufferScope;
    private const int InternalBufferSize = 256;

    public void Dispose()
    {
        
    }
}
