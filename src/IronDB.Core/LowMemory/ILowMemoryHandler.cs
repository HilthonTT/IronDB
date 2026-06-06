namespace IronDB.Core.LowMemory;

public interface ILowMemoryHandler
{
    void LowMemory(LowMemorySeverity lowMemorySeverity);
    void LowMemoryOver();
}