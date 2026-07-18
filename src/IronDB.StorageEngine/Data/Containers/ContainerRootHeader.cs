using System.Runtime.InteropServices;

namespace IronDB.StorageEngine.Data.Containers;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ContainerRootHeader
{
    [FieldOffset(0)]
    public RootObjectType RootObjectType;

    [FieldOffset(1)]
    public long ContainerId;
}