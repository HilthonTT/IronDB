namespace IronDB.Core.Utils;

public static class Sizes
{
    private const double GB = 1024 * 1024 * 1024;
    private const double MB = 1024 * 1024;
    private const double KB = 1024;

    public static string? Humane(long? bytes)
    {
        if (bytes is null)
        {
            return null;
        }

        var absSize = Math.Abs(bytes.Value);
        if (absSize >= GB)
        {
            return string.Format("{0:#,#.##} GBytes", bytes / GB);
        }
        if (absSize >= MB)
        {
            return string.Format("{0:#,#.##} MBytes", bytes / MB);
        }
        if (absSize >= KB)
        {
            return string.Format("{0:#,#.##} KBytes", bytes / KB);
        }
        return string.Format("{0:#,#0} Bytes", bytes);
    }
}
