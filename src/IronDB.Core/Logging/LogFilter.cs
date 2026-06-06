using IronDB.Core.Json.Parsing;

namespace IronDB.Core.Logging;

public sealed class LogFilter : IDynamicJson
{
    internal LogFilter()
    {
        // for deserialization
    }

    public LogFilter(LogLevel minLevel, LogLevel maxLevel, string condition, LogFilterAction action)
    {
        MinLevel = minLevel;
        MaxLevel = maxLevel;
        Condition = condition;
        Action = action;
    }

    public LogLevel MinLevel { get; internal set; }

    public LogLevel MaxLevel { get; internal set; }

    public string Condition { get; internal set; } = string.Empty;

    public LogFilterAction Action { get; internal set; }

    public DynamicJsonValue ToJson()
    {
        return new DynamicJsonValue
        {
            [nameof(MinLevel)] = MinLevel,
            [nameof(MaxLevel)] = MaxLevel,
            [nameof(Condition)] = Condition,
            [nameof(Action)] = Action
        };
    }
}