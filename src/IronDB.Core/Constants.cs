namespace IronDB.Core;

public static class Constants
{
    internal static class Size
    {
        public const int Kilobyte = 1024;
        public const int Megabyte = 1024 * Kilobyte;
        public const int Gigabyte = 1024 * Megabyte;
        public const long Terabyte = 1024 * (long)Gigabyte;
    }

    public sealed class Logging
    {
        private Logging()
        {
        }

        public const string DefaultHeaderAndFooterLayout = "Date|Level|ThreadID|Resource|Component|Logger|Message|Data";

        public const string DefaultLayout = "${longdate:universalTime=true}|${level:uppercase=true}|${threadid}|${event-properties:item=Resource}|${event-properties:item=Component}|${logger}|${message:withexception=true}|${event-properties:item=Data}";

        public sealed class Properties
        {
            private Properties()
            {
            }

            public const string Resource = "Resource";

            public const string Component = "Component";

            public const string Data = "Data";
        }

        public sealed class Names
        {
            private Names()
            {
            }

            public const string ConsoleRuleName = "Iron_Console";

            public const string PipeRuleName = "Iron_Pipe";

            public const string AdminLogsRuleName = "Iron_WebSocket";

            public const string MicrosoftRuleName = "Iron_Microsoft";

            public const string SystemRuleName = "Iron_System";

            public const string DefaultRuleName = "Iron_Default";

            public const string DefaultAuditRuleName = "Iron_Default_Audit";
        }
    }

    internal static class Naming
    {
        public const string VectorPropertyName = "@vector";

        public static ReadOnlySpan<byte> VectorPropertyNameAsSpan => "@vector"u8;
    }
}
