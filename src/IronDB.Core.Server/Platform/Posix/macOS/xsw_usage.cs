namespace IronDB.Core.Server.Platform.Posix.macOS;

// Native layout: fields are written by sysctlbyname("vm.swapusage").
#pragma warning disable CS0649 // never assigned to

internal struct xsw_usage
{
    public ulong xsu_total;
    public ulong xsu_avail;
    public ulong xsu_used;
    public uint xsu_pagesize;
    public bool xsu_encrypted;
};

#pragma warning restore CS0649