namespace IronDB.Core;

public interface IReadOnlySpanIndexer
{
    int Length { get; }

    bool IsNull(int i);

    ReadOnlySpan<byte> this[int i] { get; }
}
