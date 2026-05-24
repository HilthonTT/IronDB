using System.Globalization;
using IronDB.Common.Utils;
using Serilog;

namespace IronDB.Transport.Tcp.Framing;

public sealed class LengthPrefixMessageFramer : IMessageFramer<ArraySegment<byte>>
{
    private static readonly ILogger Logger = Log.ForContext<LengthPrefixMessageFramer>();

    public const int HeaderLength = sizeof(int);

    private readonly int _maxPackageSize;

    private byte[] _messageBuffer = [];
    private int _bufferIndex;
    private int _headerBytes;
    private int _packageLength;
    private Action<ArraySegment<byte>>? _receivedHandler;

    public bool HasData => _headerBytes > 0;

    public LengthPrefixMessageFramer(int maxPackageSize = 64 * 1024 * 1024)
    {
        Ensure.Positive(maxPackageSize);
        _maxPackageSize = maxPackageSize;
    }

    public IEnumerable<ArraySegment<byte>> FrameData(ArraySegment<byte> data)
    {
        int length = data.Count;

        yield return new ArraySegment<byte>(
            [(byte)length, (byte)(length >> 8), (byte)(length >> 16), (byte)(length >> 24)]);

        yield return data;
    }

    public void RegisterMessageArrivedCallback(Action<ArraySegment<byte>> handler)
    {
        Ensure.NotNull(handler);
        _receivedHandler = handler;
    }

    public void Reset()
    {
        _messageBuffer = [];
        _headerBytes = 0;
        _packageLength = 0;
        _bufferIndex = 0;
    }

    public void UnFrameData(IEnumerable<ArraySegment<byte>> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        foreach (var buffer in data)
        {
            Parse(buffer);
        }
    }

    public void UnFrameData(ArraySegment<byte> data) => Parse(data);

    private void Parse(ArraySegment<byte> bytes)
    {
        byte[]? data = bytes.Array;
        if (data is null)
        {
            return;
        }

        for (int i = bytes.Offset, n = bytes.Offset + bytes.Count; i < n; i++)
        {
            if (_headerBytes < HeaderLength)
            {
                _packageLength |= data[i] << (_headerBytes * 8); // little-endian order
                ++_headerBytes;
                if (_headerBytes == HeaderLength)
                {
                    if (_packageLength <= 0 || _packageLength > _maxPackageSize)
                    {
                        Logger.Error("FRAMING ERROR! Data:\n {data}", Helper.FormatBinaryDump(bytes));
                        throw new PackageFramingException(string.Format(
                            CultureInfo.InvariantCulture,
                            "Package size is out of bounds: {0} (max: {1}).",
                            _packageLength, _maxPackageSize));
                    }

                    _messageBuffer = new byte[_packageLength];
                }
            }
            else
            {
                int copyCnt = Math.Min(bytes.Count + bytes.Offset - i, _packageLength - _bufferIndex);
                Buffer.BlockCopy(data, i, _messageBuffer, _bufferIndex, copyCnt);
                _bufferIndex += copyCnt;
                i += copyCnt - 1;

                if (_bufferIndex == _packageLength)
                {
                    _receivedHandler?.Invoke(new ArraySegment<byte>(_messageBuffer, 0, _bufferIndex));
                    _messageBuffer = [];
                    _headerBytes = 0;
                    _packageLength = 0;
                    _bufferIndex = 0;
                }
            }
        }
    }
}
