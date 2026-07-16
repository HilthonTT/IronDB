namespace IronDB.Core.Server.Utils.DiskStatsGetter;

internal sealed class NotImplementedDiskStatsGetter : IDiskStatsGetter
{
    public DiskStatsResult? Get(string? drive) => null;

    public Task<DiskStatsResult?>? GetAsync(string? drive) => null;

    public static void Dispose()
    {
    }
}