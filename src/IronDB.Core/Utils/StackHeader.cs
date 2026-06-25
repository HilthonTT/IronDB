namespace IronDB.Core.Utils;

public class StackHeader<T>
    where T : class
{
    public static readonly StackNode<T> HeaderDisposed = new();

    public StackNode<T>? Head { get; set; }
}
