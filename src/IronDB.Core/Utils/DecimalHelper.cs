using System.Linq.Expressions;
using System.Reflection;

namespace IronDB.Core.Utils;

internal delegate bool IsDoubleDelegate(ref decimal value);

internal sealed class DecimalHelper
{
    private static readonly string FlagsFieldName;

    public static readonly DecimalHelper Instance = new();

    public readonly IsDoubleDelegate IsDouble;

    static DecimalHelper()
    {
        var decimalType = typeof(decimal);
        if (decimalType.GetField("_flags", BindingFlags.Instance | BindingFlags.NonPublic) is not null)
        {
            FlagsFieldName = "_flags";
        }
        else if (decimalType.GetField("flags", BindingFlags.Instance | BindingFlags.NonPublic) is not null)
        {
            FlagsFieldName = "flags";
        }
        else
        {
            throw new InvalidOperationException("Could not determine name of the 'flags' field in decimal type");
        }

        Instance = new DecimalHelper();
    }

    public DecimalHelper()
    {
        IsDouble = CreateIsDoubleMethod().Compile();
    }

    private static Expression<IsDoubleDelegate> CreateIsDoubleMethod()
    {
        ParameterExpression value = Expression.Parameter(typeof(decimal).MakeByRefType(), "value");

        BinaryExpression digits = Expression.RightShift(
            Expression.And(Expression.Field(value, FlagsFieldName), Expression.Constant(~int.MinValue, typeof(int))),
            Expression.Constant(16, typeof(int)));

        BinaryExpression hasDecimal = Expression.NotEqual(digits, Expression.Constant(0));

        return Expression.Lambda<IsDoubleDelegate>(hasDecimal, value);
    }
}
