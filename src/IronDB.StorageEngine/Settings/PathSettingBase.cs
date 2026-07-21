namespace IronDB.StorageEngine.Settings;

public abstract class PathSettingBase<T>
{
    protected readonly PathSettingBase<T>? _baseDataDir;
    protected readonly string _path;

    protected string? _fullPath;

    protected PathSettingBase(string path, PathSettingBase<T>? baseDataDir = null)
    {
        ValidatePath(path);
        _baseDataDir = baseDataDir;
        _path = path;
    }

    public static void ValidatePath(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) &&
            (path.StartsWith("appdrive:", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith('~') ||
            path.StartsWith("$home", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"The path '{path}' is illegal! Paths in RavenDB can't start with 'appdrive:', '~' or '$home'");
        }
    }

    public string FullPath => _fullPath ??= ToFullPath();

    public abstract T Combine(string path);

    public abstract T Combine(T path);

    public string ToFullPath()
    {
        return PathUtil.ToFullPath(_path, _baseDataDir?.FullPath);
    }

    public override string ToString()
    {
        return FullPath;
    }

    protected bool Equals(PathSettingBase<T> other)
    {
        return FullPath == other.FullPath;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((PathSettingBase<T>)obj);
    }

    public override int GetHashCode()
    {
        return FullPath.GetHashCode();
    }
}
