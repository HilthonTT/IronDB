namespace IronDB.Core.Server.Settings;

public class IronPathSetting(string path, string? baseDataDir = null) 
    : PathSettingBase<IronPathSetting>(path, baseDataDir != null ? new IronPathSetting(baseDataDir) : null)
{
    public override IronPathSetting Combine(string path)
    {
        return new IronPathSetting(Path.Combine(_path, path), _baseDataDir?.FullPath);
    }

    public override IronPathSetting Combine(IronPathSetting path)
    {
        return new IronPathSetting(Path.Combine(_path, path._path), _baseDataDir?.FullPath);
    }
}
