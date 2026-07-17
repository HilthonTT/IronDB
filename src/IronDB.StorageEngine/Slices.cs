using IronDB.Core.Server.Unmanaged;
using IronDB.Core.Threading;

namespace IronDB.StorageEngine;

public static class Slices
{
    private static readonly ByteStringContext SharedSliceContent = new ByteStringContext(SharedMultipleUseFlag.None);

    public static readonly Slice AfterAllKeys;
    public static readonly Slice BeforeAllKeys;
    public static readonly Slice Empty;

    static Slices()
    {
        SharedSliceContent.From(string.Empty, out ByteString empty);
        Empty = new Slice(SliceOptions.Key, empty);
        SharedSliceContent.From(string.Empty, out ByteString before);
        BeforeAllKeys = new Slice(SliceOptions.BeforeAllKeys, before);
        SharedSliceContent.From(string.Empty, out ByteString after);
        AfterAllKeys = new Slice(SliceOptions.AfterAllKeys, after);
    }
}
