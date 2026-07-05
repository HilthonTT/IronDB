namespace IronDB.Core.Server.Utils;

internal sealed class StrongReference<T>
{
    public T Value { get; set; } = default!;
}