namespace IronDB.BufferManagement;

/// <summary>
/// Thrown when a buffer presented to a <see cref="BufferManager"/> or
/// <see cref="BufferPool"/> is malformed or has the wrong chunk size.
/// </summary>
public sealed class InvalidBufferException : Exception
{
    public InvalidBufferException()
    {
    }

    public InvalidBufferException(string message)
        : base(message)
    {
    }

    public InvalidBufferException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
