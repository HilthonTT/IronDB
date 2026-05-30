using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace IronDB.Core.Extensions;

internal static class TypeExtensions
{
    internal const string RecordEqualityContractPropertyName = "EqualityContract";

    public static string GetTypeNameForSerialization(this Type? t)
    {
        return RemoveAssemblyDetails(t?.AssemblyQualifiedName);
    }

    internal static bool IsRecord(this Type? type)
    {
        if (type is null)
        {
            return false;
        }

        var equalityContractProperty = type.GetProperty(RecordEqualityContractPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (equalityContractProperty is null)
        {
            return false;
        }

        MethodInfo? getMethod = equalityContractProperty.GetGetMethod(nonPublic: true);
        if (getMethod is null)
        {
            return false;
        }

        return getMethod?.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is not null;
    }

    private static string RemoveAssemblyDetails(string? fullyQualifiedTypeName)
    {
        if (string.IsNullOrWhiteSpace(fullyQualifiedTypeName))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        // loop through the type name and filter out qualified assembly details from nested type names
        bool writingAssemblyName = false;
        bool skippingAssemblyDetails = false;

        for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
        {
            char current = fullyQualifiedTypeName[i];
            switch (current)
            {
                case '[':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    builder.Append(current);
                    break;

                case ']':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    builder.Append(current);
                    break;

                case ',':
                    if (!writingAssemblyName)
                    {
                        writingAssemblyName = true;
                        builder.Append(current);
                    }
                    else
                    {
                        skippingAssemblyDetails = true;
                    }
                    break;

                default:
                    if (!skippingAssemblyDetails)
                    {
                        builder.Append(current);
                    }
                    break;
            }
        }

        return builder.ToString();
    }
}
