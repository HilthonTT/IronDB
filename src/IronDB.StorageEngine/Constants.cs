using IronDB.StorageEngine.Data.BTrees;

namespace IronDB.StorageEngine;

public sealed unsafe class Constants
{
    public const string RootTreeName = "$Root";
    public static readonly Slice RootTreeNameSlice;

    public const string MetadataTreeName = "$Database-Metadata";
    public static readonly Slice MetadataTreeNameSlice;

    public const string DatabaseFilename = "Iron.Engine";
    public static readonly Slice DatabaseFilenameSlice;

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

    public static class Tree
    {
        public const int PageHeaderSize = TreePageHeader.SizeOf;
        public const int NodeHeaderSize = TreeNodeHeader.SizeOf;
        public const int NodeOffsetSize = sizeof(ushort);
        public const int PageNumberSize = sizeof(long);

        /// <summary>
        /// If there are less than 2 keys in a page, we no longer have a tree
        /// This impacts the MaxKeySize available
        /// </summary>
        public const int MinKeysInPage = 2;

        static Tree()
        {
            Assert(() => PageHeaderSize == sizeof(TreePageHeader), () => $"{nameof(TreePageHeader)} size has changed and not updated at Voron.Global.Constants.");
            Assert(() => NodeHeaderSize == sizeof(TreeNodeHeader), () => $"{nameof(TreeNodeHeader)} size has changed and not updated at Voron.Global.Constants.");
        }
    }

    public static void Assert(Func<bool> condition, Func<string> reason)
    {
        if (!condition())
        {
            throw new NotSupportedException($"Critical: A constant assertion has failed. Reason: {reason()}.");
        }
    }
}
