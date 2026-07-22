using System.Runtime.CompilerServices;

namespace IronDB.StorageEngine.Impl.Paging;

public sealed class CryptoTransactionState
{
    private Dictionary<long, EncryptionBuffer> _loadedBuffers = [];
    private long _totalCryptoBufferSize;

    /// <summary>
    /// Used for computing the total memory used by the transaction crypto buffers
    /// </summary>
    public long TotalCryptoBufferSize => _totalCryptoBufferSize;

    public void SetBuffers(Dictionary<long, EncryptionBuffer> loadedBuffers)
    {
        var total = 0L;
        foreach (var buffer in loadedBuffers.Values)
        {
            total += buffer.Size;
        }

        _loadedBuffers = loadedBuffers;
        _totalCryptoBufferSize = total;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(long pageNumber, out EncryptionBuffer? value)
    {
        return _loadedBuffers.TryGetValue(pageNumber, out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveBuffer(long pageNumber)
    {
        return _loadedBuffers.Remove(pageNumber);
    }

    public EncryptionBuffer this[long index]
    {
        get => _loadedBuffers[index];
        //This assumes that we don't replace buffers just set them.
        set
        {
            _loadedBuffers[index] = value;
            _totalCryptoBufferSize += value.Size;
        }
    }

    public IEnumerator<KeyValuePair<long, EncryptionBuffer>> GetEnumerator()
    {
        return _loadedBuffers.GetEnumerator();
    }
}
