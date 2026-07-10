namespace IronDB.Core.Server.Platform.Posix.macOS;

public unsafe struct host_cpu_load_info
{
    public fixed uint cpu_ticks[(int)CpuState.CPU_STATE_MAX]; /* number of ticks while running... */
}
