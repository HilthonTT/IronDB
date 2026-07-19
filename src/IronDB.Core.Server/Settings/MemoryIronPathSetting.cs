namespace IronDB.Core.Server.Settings;

public sealed class MemoryIronPathSetting : IronPathSetting
{
    public MemoryIronPathSetting() : base(":memory:")
    {
        _fullPath = _path;
    }
}
