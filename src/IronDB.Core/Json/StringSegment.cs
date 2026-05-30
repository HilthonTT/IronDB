// STUB: minimal surface to satisfy compiler.

namespace IronDB.Core.Json;

public readonly struct StringSegment : IEquatable<StringSegment>
{
    public string Value { get; }

    public StringSegment(string value)
    {
        Value = value;
    }

    public ReadOnlySpan<char> AsSpan() => Value.AsSpan();

    public bool Equals(StringSegment other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is StringSegment other && Equals(other);

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public static implicit operator StringSegment(string value) => new(value);

    public static implicit operator string(StringSegment value) => value.Value;
}
