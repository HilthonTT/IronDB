using System.Runtime.InteropServices;

namespace IronDB.StorageEngine.Data;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct RootHeader
{
    [FieldOffset(0)]
    public RootObjectType RootObjectType;
}
