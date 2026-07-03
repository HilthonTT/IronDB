namespace IronDB.Core.Utils;

public static class ThreadLocalCleanup
{
    public static event Action ReleaseThreadLocalState = delegate { };

    public static void Run()
    {
        ReleaseThreadLocalState();
    }
}
