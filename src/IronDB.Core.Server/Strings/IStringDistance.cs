namespace IronDB.Core.Server.Strings;

public interface IStringDistance
{
    float GetDistance(ReadOnlySpan<byte> target, ReadOnlySpan<byte> other);
}
