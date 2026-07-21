namespace IronDB.StorageEngine.Exceptions;

public sealed class ConcurrencyErrorException : ErrorException
{
    public ConcurrencyErrorException()
    {
    }

    public ConcurrencyErrorException(string message) : base(message)
    {
    }

    public ConcurrencyErrorException(string message, Exception inner) : base(message, inner)
    {
    }
}
