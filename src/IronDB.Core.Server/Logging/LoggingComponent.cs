namespace IronDB.Core.Server.Logging;

public sealed class LoggingComponent(string name)
{
    private readonly string _name = name;

    public override string ToString()
    {
        return _name;
    }

    public static readonly LoggingComponent Tcp = new("TCP");

    public static readonly LoggingComponent Configuration = new("Configuration");

    public static readonly LoggingComponent ServerStore = new("ServerStore");

    public static LoggingComponent RemoteConnection(string src, string dest)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(src, nameof(src));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(dest, nameof(dest));

        return new($"{src} > {dest}");
    }

    public static LoggingComponent Name(string name)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        return new(name);
    }

    public static LoggingComponent NodeTag(string nodeTag)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(nodeTag, nameof(nodeTag));
        return new(nodeTag);
    }
}
