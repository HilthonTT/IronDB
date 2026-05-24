namespace IronDB.BufferManagement;

public sealed class UnableToAllocateBufferException : Exception
{
    public UnableToAllocateBufferException()
        : base("Cannot allocate buffer after few trials.")
    {
    }
}
