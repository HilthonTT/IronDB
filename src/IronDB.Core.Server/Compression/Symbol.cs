using IronDB.Core.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IronDB.Core.Server.Compression;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct Symbol
{
    private uint _startKey;
    private readonly byte _length;

    public Symbol(in ReadOnlySpan<byte> startKey)
    {
        Debug.Assert(startKey.Length <= 4);

        Span<byte> aux = stackalloc byte[4];
        startKey.CopyTo(aux);
        var intAux = MemoryMarshal.Cast<byte, uint>(aux);
        _startKey = intAux[0];

        _length = (byte)startKey.Length;
    }

    public readonly uint StartKeyAsInt => Bits.SwapBytes(_startKey);

    public Span<byte> StartKey => new(Unsafe.AsPointer(ref _startKey), _length);

    public readonly int Length => _length;

    public override string ToString() => $"({Length},{Encoding.ASCII.GetString(StartKey)})";
}