namespace IronDB.Core;

internal static class Constants
{
    internal static class Size
    {
        public const int Kilobyte = 1024;
        public const int Megabyte = 1024 * Kilobyte;
        public const int Gigabyte = 1024 * Megabyte;
        public const long Terabyte = 1024 * (long)Gigabyte;
    }

    internal sealed class Logging
    {
        private Logging()
        {
        }

        internal const string DefaultHeaderAndFooterLayout = "Date|Level|ThreadID|Resource|Component|Logger|Message|Data";

        internal const string DefaultLayout = "${longdate:universalTime=true}|${level:uppercase=true}|${threadid}|${event-properties:item=Resource}|${event-properties:item=Component}|${logger}|${message:withexception=true}|${event-properties:item=Data}";

        internal sealed class Properties
        {
            private Properties()
            {
            }

            internal const string Resource = "Resource";

            internal const string Component = "Component";

            internal const string Data = "Data";
        }

        internal sealed class Names
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
    }

    internal static class Naming
    {
        public const string VectorPropertyName = "@vector";

        public static ReadOnlySpan<byte> VectorPropertyNameAsSpan => "@vector"u8;
    }
}
