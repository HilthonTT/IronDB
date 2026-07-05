using IronDB.Core.Logging;
using NLog.Filters;

namespace IronDB.Core.Server.Logging;

internal static class LogFilterExtensions
{
    public static FilterResult ToNLogFilterResult(this LogFilterAction action)
    {
        return action switch
        {
            LogFilterAction.Neutral => FilterResult.Neutral,
            LogFilterAction.Log => FilterResult.Log,
            LogFilterAction.Ignore => FilterResult.Ignore,
            LogFilterAction.LogFinal => FilterResult.LogFinal,
            LogFilterAction.IgnoreFinal => FilterResult.IgnoreFinal,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }

    public static LogFilterAction ToLogFilterAction(this FilterResult result)
    {
        return result switch
        {
            FilterResult.Neutral => LogFilterAction.Neutral,
            FilterResult.Log => LogFilterAction.Log,
            FilterResult.Ignore => LogFilterAction.Ignore,
            FilterResult.LogFinal => LogFilterAction.LogFinal,
            FilterResult.IgnoreFinal => LogFilterAction.IgnoreFinal,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }
}
