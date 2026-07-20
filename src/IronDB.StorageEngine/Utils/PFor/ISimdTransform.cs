using System.Runtime.Intrinsics;

namespace IronDB.StorageEngine.Utils.PFor;

public interface ISimdTransform
{
    Vector256<uint> Encode(Vector256<uint> curr, ref Vector256<uint> prev);

    Vector256<uint> Decode(Vector256<uint> curr, ref Vector256<uint> prev);
}
