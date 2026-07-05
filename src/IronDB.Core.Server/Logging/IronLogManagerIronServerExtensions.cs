using IronDB.Core.Logging;
using NLog;

namespace IronDB.Core.Server.Logging;

internal static class IronLogManagerIronServerExtensions
{
    public static IronLogger GetLoggerForIronServer<T>(this IronLogManager logManager) =>
        GetLoggerForIronServer(logManager, typeof(T));

    public static IronLogger GetLoggerForIronServer(this IronLogManager _, Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        ArgumentNullException.ThrowIfNull(type.FullName, nameof(type.FullName));

        return new IronLogger(LogManager.GetLogger(type.FullName)
            .WithProperty(Core.Constants.Logging.Properties.Resource, "Sparrow"));
    }
}
