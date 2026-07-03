using IronDB.Core.Platform;
using NLog.Layouts;

namespace IronDB.Core.Server;

internal static class Constants
{
    internal static class Size
    {
        public const int Kilobyte = 1024;
        public const int Megabyte = 1024 * Kilobyte;
        public const int Gigabyte = 1024 * Megabyte;
        public const long Terabyte = 1024 * (long)Gigabyte;
    }

    internal static class Encryption
    {
        public static readonly int XChachaAdLen = (int)Sodium.crypto_secretstream_xchacha20poly1305_abytes();
        public const int DefaultBufferSize = 4096;
    }

    internal static class Logging
    {
        internal static List<JsonAttribute> DefaultAdminLogsJsonAttributes =
        [
            new JsonAttribute("Date", "${longdate}"),
            new JsonAttribute("Level", "${level:uppercase=true}"),
            new JsonAttribute("ThreadID", "${threadid}"),
            new JsonAttribute("Resource", "${event-properties:item=Resource}"),
            new JsonAttribute("Component", "${event-properties:item=Component}"),
            new JsonAttribute("Logger", "${logger}"),
            new JsonAttribute("Message", "${message:withexception=true}"),
            new JsonAttribute("Data", "${event-properties:item=Data}"),
        ];
    }

    internal class Names
    {
        private Names()
        {
        }

        internal const string ConsoleRuleName = "Iron_Console";

        internal const string PipeRuleName = "Iron_Pipe";

        internal const string AdminLogsRuleName = "Iron_WebSocket";

        internal const string MicrosoftRuleName = "Iron_Microsoft";

        internal const string SystemRuleName = "Iron_System";

        internal const string DefaultRuleName = "Iron_Default";

        internal const string DefaultAuditRuleName = "Iron_Default_Audit";
    }

    internal static class Naming
    {
        public const string VectorPropertyName = "@vector";

        public static ReadOnlySpan<byte> VectorPropertyNameAsSpan => "@vector"u8;
    }
}
