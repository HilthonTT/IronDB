namespace IronDB.StorageEngine.Exceptions;

public sealed class QuotaException : Exception
{
    public QuotaException()
    {

    }

    public QuotaException(string message)
        : base(message)
    {
    }

    public QuotaException(string message, Exception inner)
        : base(message, inner)
    {

    }
}