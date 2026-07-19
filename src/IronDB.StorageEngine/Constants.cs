namespace IronDB.StorageEngine;

public sealed class Constants
{
    public static class Size
    {
        public const int Kilobyte = 1024;
        public const int Megabyte = 1024 * Kilobyte;
        public const int Gigabyte = 1024 * Megabyte;
        public const long Terabyte = 1024 * (long)Gigabyte;

        public const int Sector = 512;
    }

    public static class Storage
    {
        public const int PageSize = 8 * Size.Kilobyte;

        static Storage()
        {
            GC.KeepAlive(Array.Empty<int>());
        }
    }
}
