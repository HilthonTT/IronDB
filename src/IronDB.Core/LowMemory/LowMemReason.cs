namespace IronDB.Core.LowMemory;

internal enum LowMemReason
{
    None,
    LowMemOnTimeoutChk,
    BackToNormal,
    BackToNormalSimulation,
    LowMemStateSimulation,
    BackToNormalHandler,
    LowMemHandler
}
