namespace IronDB.Core.Debugging;

internal static class DebugStuff
{
    public static IElectricFencedMemory ElectricFencedMemory = default!;

    internal interface IElectricFencedMemory
    {
        void IncrementContext();

        void DecrementContext();

        //void RegisterContextAllocation(JsonOperationContext context, string stackTrace);

        //void UnregisterContextAllocation(JsonOperationContext context);
    }
}