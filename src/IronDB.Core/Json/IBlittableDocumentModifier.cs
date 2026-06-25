using IronDB.Core.Json.Parsing;

namespace IronDB.Core.Json;

/// <summary>Hook invoked while building a blittable document to mutate it mid-parse.</summary>
public interface IBlittableDocumentModifier
{
    void StartObject();

    void EndObject();

    void Reset(JsonOperationContext context);

    bool AboutToReadPropertyName(IJsonParser reader, JsonParserState state);
}
