namespace IronDB.Core.Logging;

internal static class IronManagerCoreExtensions
{
    public static IIronLogger? GetLogger<T>(this IronLogManager logManager) => GetLogger(logManager, typeof(T));

    public static IIronLogger? GetLogger(this IronLogManager logManager, Type? type)
    {
        try
        {
            if (type is null || string.IsNullOrWhiteSpace(type.FullName))
            {
                return null;
            }

            return logManager.GetLogger(type)?.WithProperty(Constants.Logging.Properties.Resource, "Core");
        }
        catch
        {
            return null;
        }
    }
}