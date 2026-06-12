namespace IronDB.Core.Utils;

internal sealed class StackNode<T>
{
    public T? Value { get; set; }

    public StackNode<T>? Next { get; set; }
}