namespace IronDB.Core.Utils;

internal sealed class StackHeader<T>
{
    public static readonly StackNode<T> HeaderDisposed = new();

    public StackNode<T>? Head { get; set; }
}
