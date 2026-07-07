using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IronDB.Core.Server.Compression;

internal unsafe struct SymbolFrequency
{
    private uint _startKey;
    public int Frequency;
    public int Length;

    public SymbolFrequency(in ReadOnlySpan<byte> startKey, int frequency)
    {
        Debug.Assert(startKey.Length <= 4);

        Span<byte> aux = stackalloc byte[4];
        startKey.CopyTo(aux);
        var intAux = MemoryMarshal.Cast<byte, uint>(aux);
        _startKey = intAux[0];

        Length = startKey.Length;
        Frequency = frequency;
    }

    public ReadOnlySpan<byte> StartKey => new(Unsafe.AsPointer(ref _startKey), Length);

    public override string ToString() => $"(Freq={Frequency}|{Length},{Encoding.ASCII.GetString(StartKey)})";
}
