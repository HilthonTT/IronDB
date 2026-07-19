using System.Diagnostics.CodeAnalysis;

namespace IronDB.Core.Server.Unmanaged;

public sealed class ByteStringContentComparer : IEqualityComparer<ByteString>
{
    public static ByteStringContentComparer Instance = new();

    public static bool Equals(ByteString? x, ByteString? y)
    {
        return x?.Match(y) ?? false;
    }

    public bool Equals(ByteString x, ByteString y)
    {
        return ByteStringContentComparer.Equals(x, y);
    }

    public int GetHashCode([DisallowNull] ByteString obj)
    {
        return obj.GetHashCode();
    }
}