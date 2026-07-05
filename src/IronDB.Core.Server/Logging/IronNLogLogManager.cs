using IronDB.Core.Logging;
using NLog;

namespace IronDB.Core.Server.Logging;

public sealed class IronNLogLogManager : IIronLogManager
{
    public static readonly IronNLogLogManager Instance = new();

    private IronNLogLogManager()
    {
        LogManager.ConfigurationChanged += (sender, args) =>
        {
            ConfigurationChanged?.Invoke(sender, new IronNLogLoggingConfigurationChangedEventArgs(args));
        };
    }

    public IIronLogger GetLogger(string name) => new IronLogger(LogManager.GetLogger(name));

    public event EventHandler<IronLoggingConfigurationChangedEventArgs>? ConfigurationChanged;

    public void Shutdown()
    {
        LogManager.Shutdown();
    }
}
