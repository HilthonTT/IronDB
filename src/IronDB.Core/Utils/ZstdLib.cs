using System.Security;

namespace IronDB.Core.Utils;

[SuppressUnmanagedCodeSecurity]
internal static unsafe class ZstdLib
{
    private const string LIBZSTD = "libzstd";

    internal static Func<string, Exception> CreateDictionaryException = message =>
        new InvalidOperationException(message);
}
