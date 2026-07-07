using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IronDB.Core.Server.Compression;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SymbolCode
{
    private uint _startKey;
    public int Length;
    public Code Code;

    public SymbolCode(in ReadOnlySpan<byte> startKey, in Code code)
    {
        Debug.Assert(startKey.Length <= 4);

        Span<byte> aux = stackalloc byte[4];
        startKey.CopyTo(aux);
        var intAux = MemoryMarshal.Cast<byte, uint>(aux);
        _startKey = intAux[0];

        Code = code;
        Length = startKey.Length;

        _startKey = 0;
        startKey.CopyTo(StartKey);
    }

    public Span<byte> StartKey => new(Unsafe.AsPointer(ref _startKey), Length);

    public override string ToString() => $"(Code={Code.Length},{Code.Value}|{Length}, {Encoding.ASCII.GetString(StartKey)})";
}
