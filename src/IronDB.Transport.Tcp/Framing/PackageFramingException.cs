namespace IronDB.Transport.Tcp.Framing;

public sealed class PackageFramingException(string message) : Exception(message)
{
}