using System.Runtime.InteropServices;

namespace IronDB.StorageEngine.Data.Containers;

// Represents an entry allocated inside a container (the payload slot). Value 0 means "no entry" here,
// which collided with ContainerId == 0 (root header) when both were longs. Passing the wrong type would
// make us delete or read from the root instead of the payload. The dedicated type enforces the boundary.
[StructLayout(LayoutKind.Explicit, Size = sizeof(long))]
public readonly struct ContainerEntryId(long id) : IEquatable<ContainerEntryId>
{
    [FieldOffset(0)]
    private readonly long _id = id;

    public static readonly ContainerEntryId Invalid = new(-1);

    public bool IsValid => _id > 0;

    public bool IsEmpty => _id == 0;

    public static explicit operator long(ContainerEntryId entryId) => entryId._id;

    public static explicit operator ContainerEntryId(long id) => new(id);

    public bool Equals(ContainerEntryId other) => _id == other._id;

    public override bool Equals(object? obj) => obj is ContainerEntryId other && Equals(other);

    public override int GetHashCode() => _id.GetHashCode();

    public override string ToString() => $"ContainerEntryId({_id})";

    public static bool operator ==(ContainerEntryId left, ContainerEntryId right) => left._id == right._id;

    public static bool operator !=(ContainerEntryId left, ContainerEntryId right) => left._id != right._id;
}
