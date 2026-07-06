using IronDB.Core.Collections;
using IronDB.Core.Json;
using System.Collections.Concurrent;

namespace IronDB.Core.Server.Debugging;

internal static class DebugStuff
{
    public static void Attach()
    {
#if MEM_GUARD_STACK
        Core.Debugging.DebugStuff.ElectricFencedMemory = ElectricFencedMemory.Instance;
#endif
    }
}
