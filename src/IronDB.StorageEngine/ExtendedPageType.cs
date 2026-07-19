namespace IronDB.StorageEngine;

public enum ExtendedPageType : byte
{
    None = 0,
    PostingListLeaf = 1,
    PostingListBranch = 2,
    Container = 3,
    ContainerOverflow = 4,
}