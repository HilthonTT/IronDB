namespace IronDB.StorageEngine.Settings;

public sealed class MemoryEnginePathSetting : EnginePathSetting
{
    public MemoryEnginePathSetting() : base(":memory:")
    {
        _fullPath = _path;
    }
}