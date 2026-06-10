namespace IronDB.Core.Utils;

internal static class CurrentProcessorIdHelper
{
    public static int GetCurrentProcessorId()
    {
#if NETSTANDARD2_0
        return Environment.CurrentManagedThreadId % Environment.ProcessorCount;
#else
        return Thread.GetCurrentProcessorId();
#endif
    }
}
