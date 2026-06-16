using IronDB.Core.Json;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Utils;

internal static class TypeUtils
{
    private static readonly ObjectPool<HashSet<object>, VisitedResetBehavior> VisitedHashsets =
          new(() => new HashSet<object>(ReferenceEqualityComparer.Default));

    public static bool ContainsBlittableObject(this object? obj)
    {
        var visited = VisitedHashsets.Allocate();
        try
        {
            return obj.ContainsBlittableObject(visited);
        }
        finally
        {
            VisitedHashsets.Free(visited);
        }
    }

    private static bool ContainsBlittableObject(this object? obj, HashSet<object> visited)
    {
        if (obj is null)
        {
            return false;
        }

        //prevent infinite loops
        if (!visited.Add(obj))
        {
            return false;
        }

        Type type = obj.GetType();

        if (type.IsPointer || type.IsEnum || type.IsCOMObject ||
            type.IsValueType ||
            type == typeof(string) ||
            type.Assembly == typeof(object).Assembly) //obviously not what we are looking for
        {
            return false;
        }

        if (obj is BlittableJsonReaderObject || obj is BlittableJsonReaderArray)
        {
            return true;
        }


        if (obj is IEnumerable array)
        {
            foreach (var item in array)
            {
                if (item.ContainsBlittableObject(visited))
                {
                    return true;
                }
            }

            return false;
        }

#if NETCOREAPP2_1_OR_GREATER
        if (obj is ITuple tuple)
        {
            for (int i = 0; i < tuple.Length; i++)
            {
                if (tuple[i].ContainsBlittableObject(visited))
                {
                    return true;
                }
            }

            return false;
        }
#endif

        if (type.IsClass || type.IsUserDefinedStruct())
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                var item = field.GetValue(obj);
                if (item?.ContainsBlittableObject(visited) ?? false)
                {
                    return true;
                }
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var value in properties.Where(p => p.CanRead).Select(p => p.GetValue(obj)))
            {
                if (value?.ContainsBlittableObject(visited) ?? false)
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUserDefinedStruct(this Type type)
    {
        return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
    }

    private readonly struct VisitedResetBehavior : IResetSupport<HashSet<object>>
    {
        public void Reset(HashSet<object> value)
        {
            value.Clear();
        }
    }
}

