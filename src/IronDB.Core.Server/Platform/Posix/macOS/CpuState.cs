namespace IronDB.Core.Server.Platform.Posix.macOS;

public enum CpuState
{
    CPU_STATE_USER = 0,
    CPU_STATE_SYSTEM = 1,
    CPU_STATE_IDLE = 2,
    CPU_STATE_NICE = 3,
    CPU_STATE_MAX = 4,
}