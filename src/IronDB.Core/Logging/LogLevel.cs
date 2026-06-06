namespace IronDB.Core.Logging;

[Flags]
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warn = 3,
    Error = 4,
    Fatal = 5,
    Off = 6,
}
