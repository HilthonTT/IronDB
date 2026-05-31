namespace IronDB.Core;

public interface ISpanIndexer
{
    int Length { get; }

    bool IsNull(int i);

    Span<byte> this[int i] { get; }
}