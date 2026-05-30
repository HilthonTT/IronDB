#nullable disable warnings
// Nullable warnings temporarily disabled while the port stabilizes — annotations remain valid; re-enable per-region as ported.

using System.Collections;
using System.Diagnostics;

namespace IronDB.Core.Json.Parsing;

public sealed class DynamicJsonArray : IEnumerable<object>, IDisposable
{
    public bool SkipOriginalArray;
    public int SourceIndex = -1;
    public int ModificationsIndex;
    public readonly List<object> Items;
    public List<int> Removals = [];

    public DynamicJsonArray()
    {
        Items = [];
    }

    public DynamicJsonArray(IEnumerable collection)
    {
        Items = [.. collection];
    }

    public void RemoveAt(int index)
    {
        Removals.Add(index);
    }

    public void Add(object obj)
    {
        EnsureNotDocumentInArray(obj);
        Items.Add(obj);
    }

    [Conditional("DEBUG")]
    private static void EnsureNotDocumentInArray(object value)
    {
        if (value != null &&
            value.GetType().FullName == "Raven.Server.Documents.Document")
        {
            throw new InvalidOperationException("Cannot add Document to DynamicJsonArray");
        }
    }

    public int Count => Items.Count;

    public IEnumerator<object> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Clear()
    {
        Items.Clear();
    }

    public void Dispose()
    {
        foreach (var item in Items)
        {
            if (item is IDisposable toDispose)
            {
                toDispose.Dispose();
            }
        }
    }
}
