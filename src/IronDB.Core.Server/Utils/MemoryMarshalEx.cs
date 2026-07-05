using System.Buffers;
using System.Runtime.InteropServices;

namespace IronDB.Core.Server.Utils;

public static class MemoryMarshalEx
{
    public static ReadOnlyMemory<TTo> Cast<TFrom, TTo>(ReadOnlyMemory<TFrom> memory) 
        where TTo : struct 
        where TFrom : struct
    {
        return new CastedMemoryManager<TFrom, TTo>(memory).Memory;
    }

    public sealed class CastedMemoryManager<TFrom, TTo>(ReadOnlyMemory<TFrom> src) : MemoryManager<TTo>
        where TTo : struct
        where TFrom : struct
    {
        public override Span<TTo> GetSpan()
        {
            Memory<TFrom> mem = MemoryMarshal.AsMemory(src);
            return MemoryMarshal.Cast<TFrom, TTo>(mem.Span);
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            return src.Pin();
        }

        public override void Unpin()
        {
            // nothing to do here
        }

        protected override void Dispose(bool disposing)
        {
            // nothing to dispose here
        }
    }
}
