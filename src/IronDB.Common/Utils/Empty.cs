namespace IronDB.Common.Utils;

/// <summary>
/// Shared empty / singleton sentinel values. The arrays are the length-zero singletons
/// returned by <see cref="Array.Empty{T}"/>, so they are safe to share across all callers.
/// </summary>
public static class Empty
{
    public static byte[] ByteArray => Array.Empty<byte>();
    public static string[] StringArray => Array.Empty<string>();
    public static object[] ObjectArray => Array.Empty<object>();

    public static readonly Action Action = static () => { };
    public static readonly object Result = new();

    public const string Xml = "";
    public const string Json = "{}";
}
