// STUB: minimal surface to satisfy compiler. Methods throw NotImplementedException.

namespace IronDB.Core.Json;

public sealed class LazyNumberValue
{
    public LazyStringValue Inner;

    public LazyNumberValue(LazyStringValue inner)
    {
        Inner = inner;
    }

    public override string ToString() => Inner.ToString();

    public static implicit operator double(LazyNumberValue value) => throw new NotImplementedException();

    public static implicit operator float(LazyNumberValue value) => throw new NotImplementedException();

    public static implicit operator decimal(LazyNumberValue value) => throw new NotImplementedException();
}
