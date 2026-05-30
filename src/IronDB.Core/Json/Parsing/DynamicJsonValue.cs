#nullable disable warnings
// Nullable warnings temporarily disabled while the port stabilizes — annotations remain valid; re-enable per-region as ported.

using IronDB.Core.Extensions;
using System.Reflection;

namespace IronDB.Core.Json.Parsing;

public sealed class DynamicJsonValue
{
    public const string TypeFieldName = "$type";

    public int SourceIndex = -1;
    public BlittableJsonReaderObject.InsertionOrderProperties SourceProperties;

    public int ModificationsIndex = 0;
    public readonly List<(string Name, object Value)> Properties = [];
    public HashSet<int> Removals = [];
    internal readonly BlittableJsonReaderObject? _source;

    public DynamicJsonValue()
    {
    }

    public DynamicJsonValue(string key, object value)
    {
        Properties.Add((key, value));
    }

    public DynamicJsonValue(Type explicitTypeInfo)
    {
        this[TypeFieldName] = explicitTypeInfo.GetTypeNameForSerialization();
    }

    public DynamicJsonValue(BlittableJsonReaderObject source)
    {
        _source = source;

        if (_source != null)
        {
#if DEBUG
            if (_source.Modifications != null && _source.Modifications.Properties.Count != _source.Modifications.ModificationsIndex)
            {
                throw new InvalidOperationException("The source already has modifications");
            }
#endif
            _source.Modifications = this;
        }
    }

    public void Remove(string property)
    {
        if (_source == null)
        {
            throw new InvalidOperationException(
               "Cannot remove property when not setup with a source blittable json object");
        }

        var propertyIndex = _source.GetPropertyIndex(property);
        if (propertyIndex == -1)
        {
            return;
        }

        Removals ??= [];
        Removals.Add(propertyIndex);
        for (int i = 0; i < Properties.Count; i++)
        {
            if (Properties[i].Name == property)
            {
                Properties.RemoveAt(i);
                break;
            }
        }
    }

    internal void RemoveInMemoryPropertyByName(string property)
    {
        if (_source != null)
        {
            throw new InvalidOperationException(
            "Cannot remove in memory property when setup with a source blittable json object");
        }
        var index = Properties.FindIndex(x => x.Name == property);
        if (index == -1)
        {
            return;
        }
        Properties.RemoveAt(index);
    }

    public object? this[string name]
    {
        set
        {
#if DEBUG
            if (value is not null &&
                value.GetType().FullName == "Raven.Server.Documents.Document")
            {
                throw new InvalidOperationException("Cannot add Document to DynamicJsonValue");
            }
#endif
            if (_source is not null)
            {
                Remove(name);
            }
            Properties.Add((name, value!));
        }
        get
        {
            foreach (var property in Properties)
            {
                if (property.Item1 != name)
                {
                    continue;
                }

                return property.Item2;
            }

            return null;
        }
    }

    public static DynamicJsonValue? Convert<T>(IDictionary<string, T>? dictionary)
    {
        if (dictionary is null)
        {
            return null;
        }

        var djv = new DynamicJsonValue();
        foreach (var kvp in dictionary)
        {
            var json = kvp.Value as IDynamicJson;
            djv[kvp.Key] = json == null ? kvp.Value : json.ToJson();
        }
        return djv;
    }

    public static DynamicJsonValue? Convert<TK, TV>(IDictionary<TK, TV>? dictionary)
    {
        if (dictionary is null)
        {
            return null;
        }

        if (!typeof(TK).IsPrimitive)
        {
            MethodInfo? mi = typeof(TK).GetMethod(nameof(ToString), types: Type.EmptyTypes);
            if (mi?.GetBaseDefinition().DeclaringType == mi?.DeclaringType)
            {
                throw new InvalidOperationException($"{typeof(TK).FullName} must override 'ToString'");
            }
        }

        var djv = new DynamicJsonValue();
        foreach (KeyValuePair<TK, TV> kvp in dictionary)
        {
            if (kvp.Value is not IDynamicJson json)
            {
                djv[kvp.Key?.ToString()!] = kvp.Value!;
            }
            else
            {
                djv[kvp.Key?.ToString()!] = json.ToJson();
            }
        }
        return djv;
    }
}
