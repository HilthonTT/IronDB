using System.Text;

namespace IronDB.Core.Extensions;

internal static class EncodingsExtensions
{
    public static unsafe int GetBytes(this UTF8Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        if (chars.IsEmpty)
        {
            return 0;
        }

        fixed (char* charsPtr = chars)
        fixed (byte* bytesPtr = bytes)
        {
            return encoding.GetBytes(charsPtr, chars.Length, bytesPtr, bytes.Length);
        }
    }

    public static unsafe string GetString(this UTF8Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        fixed (byte* bytesPtr = bytes)
        {
            return encoding.GetString(bytesPtr, bytes.Length);
        }
    }
}
