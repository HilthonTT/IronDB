namespace IronDB.Core.Exceptions;

public sealed class MemoryInfoException : Exception
{

    public MemoryInfoException()
    {
    }

    public MemoryInfoException(string message) 
        : base(message)
    {
    }

    public MemoryInfoException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}