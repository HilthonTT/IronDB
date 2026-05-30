#nullable disable warnings
// Nullable warnings temporarily disabled while the port stabilizes — annotations remain valid; re-enable per-region as ported.

namespace IronDB.Core.Json.Parsing;

public interface IDynamicJson
{
    DynamicJsonValue ToJson();
}
