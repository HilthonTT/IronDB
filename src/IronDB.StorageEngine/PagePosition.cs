namespace IronDB.StorageEngine;

public sealed class PagePosition
{
    public readonly long TransactionId;
    public readonly long JournalNumber;
    public readonly long ScratchPage;
    public readonly int ScratchNumber;
    public readonly bool IsFreedPageMarker;
    public bool UnusedInPTT;

    public PagePosition(
        long scratchPos, 
        long transactionId, 
        long journalNumber, 
        int scratchNumber, 
        bool isFreedPageMarker = false)
    {
        ScratchPage = scratchPos;
        TransactionId = transactionId;
        JournalNumber = journalNumber;
        ScratchNumber = scratchNumber;
        IsFreedPageMarker = isFreedPageMarker;
    }

    public override bool Equals(object? obj)
    {
        return PagePositionEqualityComparer.Instance.Equals(this, obj as PagePosition);
    }

    public override int GetHashCode()
    {
        return PagePositionEqualityComparer.Instance.GetHashCode(this);
    }

    public override string ToString()
    {
        return $"ScratchPos: {ScratchPage}, TransactionId: {TransactionId}, JournalNumber: {JournalNumber}, ScratchNumber: {ScratchNumber}, IsFreedPageMarker: {IsFreedPageMarker}";
    }
}
