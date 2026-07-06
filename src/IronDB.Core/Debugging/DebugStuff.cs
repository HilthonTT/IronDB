using IronDB.Core.Json;

namespace IronDB.Core.Debugging;

public static class DebugStuff
{
    public static IElectricFencedMemory ElectricFencedMemory = default!;

    public interface IElectricFencedMemory
    {
        void IncrementContext();

        void DecrementContext();

        void RegisterContextAllocation(JsonOperationContext context, string stackTrace);

        void UnregisterContextAllocation(JsonOperationContext context);
    }
}
