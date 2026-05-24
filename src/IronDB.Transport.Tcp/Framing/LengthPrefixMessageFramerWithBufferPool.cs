using System.Globalization;
using IronDB.BufferManagement;
using IronDB.Common.Utils;
using Serilog;

namespace IronDB.Transport.Tcp.Framing;

public sealed class LengthPrefixMessageFramerWithBufferPool : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<LengthPrefixMessageFramerWithBufferPool>();

    private const int PrefixLength = sizeof(int);

    private readonly int _maxPackageSize;
    private readonly BufferManager _bufferManager;
    private BufferPool? _messageBuffer;
    private Action<BufferPool>? _receivedHandler;

    private int _headerBytes;
    private int _packageLength;

    public LengthPrefixMessageFramerWithBufferPool(
        BufferManager bufferManager,
        int maxPackageSize = 16 * 1024 * 1024)
    {
        Ensure.NotNull(bufferManager);
        Ensure.Positive(maxPackageSize);

        _bufferManager = bufferManager;
        _maxPackageSize = maxPackageSize;
    }

    public void Reset()
    {
        _messageBuffer?.Dispose();
        _messageBuffer = null;
        _headerBytes = 0;
        _packageLength = 0;
    }

    public void Dispose()
    {
        _messageBuffer?.Dispose();
        _messageBuffer = null;
    }

    public void UnFrameData(IEnumerable<ArraySegment<byte>> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        foreach (ArraySegment<byte> buffer in data)
        {
            Parse(buffer);
        }
    }

    public void UnFrameData(ArraySegment<byte> data) => Parse(data);

    /// <summary>
    /// Parses a stream chunking based on length-prefixed framing. Calls are re-entrant and hold state internally.
    /// </summary>
    /// <param name="bytes">A byte array of data to append.</param>
    private void Parse(ArraySegment<byte> bytes)
    {
        byte[]? data = bytes.Array;
        if (data is null)
        {
            return;
        }

        int end = bytes.Offset + bytes.Count;
        for (int i = bytes.Offset; i < end;)
        {
            if (_headerBytes < PrefixLength)
            {
                _packageLength |= data[i] << (_headerBytes * 8); // little-endian order
                ++_headerBytes;
                i += 1;
                if (_headerBytes == PrefixLength)
                {
                    if (_packageLength <= 0 || _packageLength > _maxPackageSize)
                    {
                        Logger.Error("FRAMING ERROR! Data:\n {data}", Helper.FormatBinaryDump(bytes));
                        throw new PackageFramingException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Package size is out of bounds: {0} (max: {1}).",
                            _packageLength, _maxPackageSize));
                    }

                    _messageBuffer = new BufferPool(_bufferManager);
                }
            }
            else
            {
                int currentLength = _messageBuffer?.Length ?? 0;
                int copyCnt = Math.Min(end - i, _packageLength - currentLength);
                _messageBuffer?.Append(data, i, copyCnt);
                i += copyCnt;

                if (_messageBuffer?.Length == _packageLength)
                {
                    _receivedHandler?.Invoke(_messageBuffer);
                    _messageBuffer = null;
                    _headerBytes = 0;
                    _packageLength = 0;
                }
            }
        }
    }

    public static IEnumerable<ArraySegment<byte>> FrameData(ArraySegment<byte> data)
    {
        int length = data.Count;

        yield return new ArraySegment<byte>(
            [(byte)length, (byte)(length >> 8), (byte)(length >> 16), (byte)(length >> 24)]);
        yield return data;
    }

    public void RegisterMessageArrivedCallback(Action<BufferPool> handler)
    {
        Ensure.NotNull(handler);
        _receivedHandler = handler;
    }
}
