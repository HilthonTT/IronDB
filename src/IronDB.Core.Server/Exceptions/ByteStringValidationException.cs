namespace IronDB.Core.Server.Exceptions;

public sealed class ByteStringValidationException : Exception
{
    public ByteStringValidationException()
    {
    }

    public ByteStringValidationException(string message)
        : base(message)
    {
    }

    public ByteStringValidationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
