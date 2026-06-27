
namespace IronDB.Core.Server.Meters;

public sealed class IoMetrics
{
    public enum MeterType
    {
        Compression,
        JournalWrite,
        DataFlush,
        DataSync,
    }
}