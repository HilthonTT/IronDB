namespace IronDB.StorageEngine.Data.Fixed;

[Flags]
public enum FixedSizeTreePageFlags : byte
{
    None = 0,
    Branch = 1,
    Leaf = 2,
    Value = 4,
}