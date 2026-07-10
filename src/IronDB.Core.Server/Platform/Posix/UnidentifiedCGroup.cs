namespace IronDB.Core.Server.Platform.Posix;

public sealed class UnidentifiedCGroup : CGroup
{
    private readonly string _errorMessage;
    private DateTime _lastLog = DateTime.MinValue;

    protected override string? MemoryLimitFileName => null;
    protected override string? MemoryUsageFileName => null;
    protected override string? MaxMemoryUsageFileName => null;

    public UnidentifiedCGroup(string errorMessage)
    {
        _errorMessage = errorMessage;
    }

    protected override string? FindCGroupPathForMemoryInternal()
    {
        if (_lastLog + TimeSpan.FromMinutes(10) < DateTime.UtcNow)
        {
            _lastLog = DateTime.UtcNow;
            if (Logger.IsInfoEnabled)
                Logger.Info(_errorMessage);
        }

        return null;
    }
}
