using IronDB.Core.Platform;
using IronDB.Core.Utils;

namespace IronDB.StorageEngine.Impl.Paging;

public sealed unsafe class EncryptionBuffer(EncryptionBuffersPool pool)
{
    public static readonly UIntPtr HashSize = Sodium.crypto_generichash_bytes();
    public static readonly int HashSizeInt = (int)Sodium.crypto_generichash_bytes();
    public byte* Pointer;
    public long Size;
    public long? OriginalSize;
    public long Generation = pool.Generation;

    public NativeMemory.ThreadStats? AllocatingThread;
    public bool SkipOnTxCommit;
    public bool ExcessStorageFromAllocationBreakup;
    public bool Modified;
    public int Usages;

    public bool CanRelease => Usages == 0;

    public void AddRef()
    {
        Usages++;
    }

    public void ReleaseRef()
    {
        Usages--;
    }
}
