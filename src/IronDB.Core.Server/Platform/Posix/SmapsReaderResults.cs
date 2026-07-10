namespace IronDB.Core.Server.Platform.Posix;

internal sealed class SmapsReaderResults
{
    public string? ResultString;
    public long Size;
    public long Rss;
    public long SharedClean;
    public long SharedDirty;
    public long PrivateClean;
    public long PrivateDirty;
    public long Swap;
}