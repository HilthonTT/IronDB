using IronDB.Core.Logging;
using IronDB.Core.Server.Logging;
using NLog;

namespace IronDB.StorageEngine.Logging;

internal static class LogManagerExtensions
{
    public static IronLogger GetLoggerForGlobalEngine<T>(this IronLogManager logManager) =>
        GetLoggerForGlobalEngine(logManager, typeof(T));

    public static IronLogger GetLoggerForGlobalEngine(this IronLogManager logManager, Type type)
    {
        return new IronLogger(LogManager.GetLogger(type.FullName ?? string.Empty)
            .WithProperty(Core.Constants.Logging.Properties.Resource, LoggingResource.Engine));
    }

    public static IronLogger GetLoggerForEngine<T>(
        this IronLogManager logManager, 
        StorageEnvironmentOptions options, 
        string filePath) => GetLoggerForEngine(logManager, typeof(T), options, filePath);

    public static IronLogger GetLoggerForEngine(
        this IronLogManager logManager, 
        Type type, 
        StorageEnvironmentOptions options, 
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        return new IronLogger(LogManager.GetLogger(type.FullName ?? string.Empty)
            .WithProperty(Core.Constants.Logging.Properties.Resource, options.LoggingResource)
            .WithProperty(Core.Constants.Logging.Properties.Component, options.LoggingComponent)
            .WithProperty(Core.Constants.Logging.Properties.Data, filePath));
    }
}
