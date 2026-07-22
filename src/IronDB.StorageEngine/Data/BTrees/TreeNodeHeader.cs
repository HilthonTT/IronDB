using System.Runtime.InteropServices;

namespace IronDB.StorageEngine.Data.BTrees;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TreeNodeHeader
{
    public const int SizeOf = 11;
}
