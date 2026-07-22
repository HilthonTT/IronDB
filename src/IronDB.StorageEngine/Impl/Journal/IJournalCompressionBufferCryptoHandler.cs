namespace IronDB.StorageEngine.Impl.Journal;

public interface IJournalCompressionBufferCryptoHandler
{
    void ZeroCompressionBuffer(IPagerLevelTransactionState tx);
}
