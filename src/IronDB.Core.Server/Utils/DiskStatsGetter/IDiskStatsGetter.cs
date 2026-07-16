namespace IronDB.Core.Server.Utils.DiskStatsGetter;

public interface IDiskStatsGetter
{
    DiskStatsResult? Get(string? drive);

    Task<DiskStatsResult?>? GetAsync(string? drive);
}
