namespace IronDB.Core.Utils;

internal static class ClientChangeVectorUtils
{
    public static long GetETagById(string changeVector, string id)
    {
        if (changeVector is null)
        {
            return 0;
        }

        ArgumentNullException.ThrowIfNull(id, nameof(id));

        int index = changeVector.IndexOf("-" + id, StringComparison.Ordinal);
        if (index == -1)
        {
            return 0;
        }

        int end = index - 1;
        int start = changeVector.LastIndexOf(':', end) + 1;

        return long.Parse(changeVector.Substring(start, end - start + 1));
    }
}
