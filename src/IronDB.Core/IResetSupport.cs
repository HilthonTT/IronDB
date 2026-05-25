namespace IronDB.Core;

public interface IResetSupport<in T> where T : class
{
    void Reset(T value);
}
