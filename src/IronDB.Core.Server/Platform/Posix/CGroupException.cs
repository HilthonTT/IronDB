namespace IronDB.Core.Server.Platform.Posix;

public sealed class CGroupException(string message) : Exception(message)
{
}