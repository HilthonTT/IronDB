using System.Globalization;
using System.Reflection;
using System.Text;

namespace IronDB.Common.Utils;

public static class Helper
{
    public static readonly UTF8Encoding UTF8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static void EatException(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            action();
        }
        catch (Exception)
        {
            // intentionally swallowed
        }
    }

    public static void EatException<TArg>(TArg arg, Action<TArg> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            action(arg);
        }
        catch (Exception)
        {
            // intentionally swallowed
        }
    }

    public static T? EatException<T>(Func<T> action, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            return action();
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public static string GetDefaultLogsDir()
    {
        string assemblyDir = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
        return Path.Combine(Path.GetDirectoryName(assemblyDir) ?? string.Empty, "es-logs");
    }

    public static string FormatBinaryDump(byte[]? logBulk)
        => FormatBinaryDump(new ArraySegment<byte>(logBulk ?? Empty.ByteArray));

    public static string FormatBinaryDump(ArraySegment<byte> logBulk)
    {
        if (logBulk.Count == 0 || logBulk.Array is null)
        {
            return "--- NO DATA ---";
        }

        var sb = new StringBuilder();
        int cur = 0;
        int len = logBulk.Count;
        int rows = (logBulk.Count + 15) / 16;
        for (int row = 0; row < rows; ++row)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:000000}:", row * 16);
            for (int i = 0; i < 16; ++i, ++cur)
            {
                if (cur >= len)
                {
                    sb.Append("   ");
                }
                else
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0:X2}", logBulk.Array[logBulk.Offset + cur]);
                }
            }

            sb.Append("  | ");
            cur -= 16;
            for (int i = 0; i < 16; ++i, ++cur)
            {
                if (cur < len)
                {
                    var b = (char)logBulk.Array[logBulk.Offset + cur];
                    sb.Append(char.IsControl(b) ? '.' : b);
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
