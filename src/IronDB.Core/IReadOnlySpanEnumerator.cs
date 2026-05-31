namespace IronDB.Core;

public interface IReadOnlySpanEnumerator
{
    int Count { get; }

    void Reset();

    bool MoveNext(out ReadOnlySpan<byte> buffer);
}
