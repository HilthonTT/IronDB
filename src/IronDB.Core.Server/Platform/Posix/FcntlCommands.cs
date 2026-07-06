namespace IronDB.Core.Server.Platform.Posix;

[Flags]
public enum FcntlCommands
{
    F_NOCACHE = 0x00000030,
    F_FULLFSYNC = 0x00000033
}
