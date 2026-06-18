namespace IronDB.Core.Json;

/// <summary>Container for a blittable JSON document held by another object.</summary>
public interface IBlittableJsonContainer
{
    BlittableJsonReaderObject? BlittableJson { get; }
}
