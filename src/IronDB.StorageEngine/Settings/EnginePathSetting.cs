namespace IronDB.StorageEngine.Settings;

public sealed class EnginePathSetting : PathSettingBase<EnginePathSetting>
{
    public EnginePathSetting(string path, string? baseDataDir = null)
        : base(path, baseDataDir != null ? new EnginePathSetting(baseDataDir) : null)
    {
    }

    public override EnginePathSetting Combine(string path)
    {
        return new EnginePathSetting(Path.Combine(_path, path), _baseDataDir?.FullPath);
    }

    public override EnginePathSetting Combine(EnginePathSetting path)
    {
        return new EnginePathSetting(Path.Combine(_path, path._path), _baseDataDir?.FullPath);
    }
}
