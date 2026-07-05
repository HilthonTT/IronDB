using IronDB.Core.Logging;
using NLog.Config;

namespace IronDB.Core.Server.Logging;

public sealed class IronNLogLoggingConfigurationChangedEventArgs : IronLoggingConfigurationChangedEventArgs
{
    public readonly LoggingConfigurationChangedEventArgs Arguments;

    public IronNLogLoggingConfigurationChangedEventArgs(LoggingConfigurationChangedEventArgs arguments)
    {
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }
}