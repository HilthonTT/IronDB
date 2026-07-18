using System.Runtime.InteropServices;

namespace IronDB.StorageEngine.Data.Containers;

// Represents the root container itself (the header page that owns freelists, lookup trees, etc.).
// Historically both container ids and entry ids were plain longs, so ContainerEntryId == 0 meant
// "empty entry" while ContainerId == 0 pointed at the storage header. Mixing them up would send
// callers to the wrong page or reuse a root as if it were an entry. This wrapper marks "root" level.
[StructLayout(LayoutKind.Explicit, Size = sizeof(long))]
public readonly struct ContainerId(long id) : IEquatable<ContainerId>
{
    [FieldOffset(0)]
    private readonly long _id = id;


    public static readonly ContainerId Invalid = new(-1);

    public bool IsValid => _id > 0;

    public bool IsEmpty => _id == 0;

    public static explicit operator long(ContainerId containerId) => containerId._id;

    public static explicit operator ContainerId(long id) => new(id);

    public bool Equals(ContainerId other) => _id == other._id;

    public override bool Equals(object? obj) => obj is ContainerId other && Equals(other);

    public override int GetHashCode() => _id.GetHashCode();

    public override string ToString() => $"ContainerId({_id})";

    public static bool operator ==(ContainerId left, ContainerId right) => left._id == right._id;
    
    public static bool operator !=(ContainerId left, ContainerId right) => left._id != right._id;
}
