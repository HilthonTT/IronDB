namespace IronDB.Core.Logging;

public enum LogFilterAction
{
    /// <summary>
    /// The filter doesn't want to decide whether to log or discard the message.
    /// </summary>
    Neutral,

    /// <summary>
    /// The message should be logged.
    /// </summary>
    Log,

    /// <summary>
    /// The message should not be logged.
    /// </summary>
    Ignore,

    /// <summary>
    /// The message should be logged and processing should be finished.
    /// </summary>
    LogFinal,

    /// <summary>
    /// The message should not be logged and processing should be finished.
    /// </summary>
    IgnoreFinal,
}