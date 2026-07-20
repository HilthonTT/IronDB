using System.Runtime.InteropServices;

namespace IronDB.StorageEngine.Utils.PFor;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PForHeader
{
    public long Baseline;
    public uint ExceptionsBitmap;
    public ushort MetadataOffset;
    public ushort ExceptionsOffset;
    public ushort SharedPrefix;
}
