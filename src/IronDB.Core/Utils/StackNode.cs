namespace IronDB.Core.Utils;

public sealed class StackNode<T>
{
    public T? Value { get; set; }

    public StackNode<T>? Next { get; set; }
}