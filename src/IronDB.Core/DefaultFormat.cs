namespace IronDB.Core;

internal static class DefaultFormat
{
    internal const string TimeOnlyFormatToWrite = "o";
    internal const string DateOnlyFormatToWrite = "o";
    internal const string DateTimeOffsetFormatsToWrite = "o";
    internal const string DateTimeFormatsToWrite = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff";

    internal static readonly string[] OnlyDateTimeFormat = 
    [
        "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
        DateTimeFormatsToWrite,
        "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff'Z'"
    ];

    /// <remarks>
    /// 'r' format is used on the in metadata, because it's delivered as http header. 
    /// </remarks>
    internal static readonly string[] DateTimeFormatsToRead = 
    [
        DateTimeOffsetFormatsToWrite,
        DateTimeFormatsToWrite,
        "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
        "r",
        "yyyy-MM-ddTHH:mm:ss.fffK",
        "yyyy-MM-ddTHH:mm:ss.FFFK",
    ];
}
