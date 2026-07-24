namespace IronDB.StorageEngine.Data.RawData;

[Flags]
public enum RawDataPageFlags : byte
{
    None = 0,
    Header = 1,
    Small = 2,
    Large = 4,
}