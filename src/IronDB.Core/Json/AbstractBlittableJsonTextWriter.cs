using System.Runtime.InteropServices;

namespace IronDB.Core.Json;

public abstract unsafe class AbstractBlittableJsonTextWriter
{
    protected readonly JsonOperationContext _context;
    protected readonly Stream _stream;
    private const byte StartObject = (byte)'{';
    private const byte EndObject = (byte)'}';
    private const byte StartArray = (byte)'[';
    private const byte EndArray = (byte)']';
    private const byte Comma = (byte)',';
    private const byte Quote = (byte)'"';
    private const byte Colon = (byte)':';

    public static ReadOnlySpan<byte> NewLineBuffer => "\r\n"u8;
    public static ReadOnlySpan<byte> NaNBuffer => "\"NaN\""u8;
    public static ReadOnlySpan<byte> PositiveInfinityBuffer => "\"Infinity\""u8;
    public static ReadOnlySpan<byte> NegativeInfinityBuffer => "\"-Infinity\""u8;

    public static readonly byte[] NullBuffer = "null"u8.ToArray();
    public static readonly byte[] TrueBuffer = "true"u8.ToArray();
    public static readonly byte[] FalseBuffer = "false"u8.ToArray();

    /// <summary><![CDATA[
    /// The original code that generates this flatten sequence.
    /// EscapeCharacters = new byte[256];
    /// for (int i = 0; i< 32; i++)
    ///     EscapeCharacters[i] = 0;
    ///
    /// for (int i = 32; i<EscapeCharacters.Length; i++)
    ///     EscapeCharacters[i] = 255;
    ///
    /// EscapeCharacters[(byte)'\b'] = (byte)'b';
    /// EscapeCharacters[(byte)'\t'] = (byte)'t';
    /// EscapeCharacters[(byte)'\n'] = (byte)'n';
    /// EscapeCharacters[(byte)'\f'] = (byte)'f';
    /// EscapeCharacters[(byte)'\r'] = (byte)'r';
    /// EscapeCharacters[(byte)'\\'] = (byte)'\\';
    /// EscapeCharacters[(byte)'/'] = (byte)'/';
    /// EscapeCharacters[(byte)'"'] = (byte)'"';
    /// ]]></summary>
    private static ReadOnlySpan<byte> EscapeCharacters =>
    [
        0,   0,   0,   0,   0,   0,   0,   0,  98, 116, 110,   0, 102, 114,   0,   0,
        0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
        255, 255,  34, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,  47,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,  92, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
    ];

    private protected readonly JsonOperationContext.MemoryBuffer _pinnedBuffer;
    private readonly byte* _buffer;

    private protected int _pos;
    private readonly JsonOperationContext.MemoryBuffer.ReturnBuffer _returnBuffer;

    private static ReadOnlySpan<byte> _controlCodeEscapes => 
        "0000000100020003000400050006000700080009000A000B000C000D000E000F0010001100120013001400150016001700180019001A001B001C001D001E001F"u8;

    internal static ReadOnlySpan<int> ControlCodeEscapes => MemoryMarshal.Cast<byte, int>(_controlCodeEscapes);

    protected AbstractBlittableJsonTextWriter(JsonOperationContext context, Stream stream)
    {
        _context = context;
        _stream = stream;

        _returnBuffer = context.GetMemoryBuffer(out _pinnedBuffer);
        _buffer = _pinnedBuffer.Address;
    }

    public void WriteObject(BlittableJsonReaderObject obj)
    {

    }

    protected virtual bool FlushInternal()
    {
        ObjectDisposedException.ThrowIf(_stream is null, "The stream was closed already.");

        if (_pos == 0)
        {
            return false;
        }

        _stream.Write(_pinnedBuffer.Memory.Memory.Span.Slice(0, _pos));
        _stream.Flush();

        _pos = 0;
        return true;
    }

    protected void DisposeInternal()
    {
        try
        {
            FlushInternal();
            _stream.Flush();
        }
        catch (ObjectDisposedException)
        {
            //we are disposing, so this exception doesn't matter
        }
        // TODO: remove when we update to .net core 3
        // https://github.com/dotnet/corefx/issues/36141
        catch (NotSupportedException e)
        {
            throw new IOException("The stream was closed by the peer.", e);
        }
        finally
        {
            _returnBuffer.Dispose();
        }
    }
}
