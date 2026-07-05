namespace IronDB.Core.Server.Logging;

public sealed class LoggingResource(string name)
{
    private readonly string _name = name;

    public override string ToString()
    {
        return _name;
    }

    public static readonly LoggingResource Server = new("Server");

    public static readonly LoggingResource Cluster = new("Cluster");

    public static readonly LoggingResource Engine = new("Engine");

    public static LoggingResource Database(string? databaseName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(databaseName, nameof(databaseName));
        return new LoggingResource(databaseName);
    }
}
