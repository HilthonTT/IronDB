using IronDB.Core.Server.Unmanaged;

namespace IronDB.StorageEngine;

public enum SliceOptions : byte
{
    Key = 0,
    BeforeAllKeys = ByteStringType.UserDefined1,
    AfterAllKeys
}