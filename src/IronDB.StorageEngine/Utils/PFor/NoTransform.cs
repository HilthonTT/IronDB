using System.Runtime.Intrinsics;

namespace IronDB.StorageEngine.Utils.PFor;

public readonly struct NoTransform : ISimdTransform
{
    public Vector256<uint> Decode(Vector256<uint> curr, ref Vector256<uint> prev)
    {
        return curr;
    }

    public Vector256<uint> Encode(Vector256<uint> curr, ref Vector256<uint> prev)
    {
        return curr;
    }
}
