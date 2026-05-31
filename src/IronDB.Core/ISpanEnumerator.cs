namespace IronDB.Core;

public interface ISpanEnumerator
{
    void Reset();
    bool MoveNext(out Span<byte> result);
}
