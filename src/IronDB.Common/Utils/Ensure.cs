using System.Runtime.CompilerServices;

namespace IronDB.Common.Utils;

public static class Ensure
{
    public static T NotNull<T>(T argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(argument, argumentName);
        return argument;
    }

    public static string NotNullOrEmpty(string argument, [CallerArgumentExpression(nameof(argument))] string? argumentName = null)
    {
        if (string.IsNullOrEmpty(argument))
        {
            throw new ArgumentException($"{argumentName} should be non-null and non-empty.", argumentName);
        }
        return argument;
    }

    public static int Positive(int number, [CallerArgumentExpression(nameof(number))] string? argumentName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number, argumentName);
        return number;
    }

    public static long Positive(long number, [CallerArgumentExpression(nameof(number))] string? argumentName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number, argumentName);
        return number;
    }

    public static long Nonnegative(long number, [CallerArgumentExpression(nameof(number))] string? argumentName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(number, argumentName);
        return number;
    }

    public static int Nonnegative(int number, [CallerArgumentExpression(nameof(number))] string? argumentName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(number, argumentName);
        return number;
    }

    public static double Nonnegative(double number, [CallerArgumentExpression(nameof(number))] string? argumentName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(number, argumentName);
        return number;
    }

    public static Guid NotEmptyGuid(Guid value, [CallerArgumentExpression(nameof(value))] string? argumentName = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{argumentName} should be a non-empty GUID.", argumentName);
        }
        return value;
    }

    public static void Equal(int expected, int actual, [CallerArgumentExpression(nameof(actual))] string? argumentName = null)
    {
        if (expected != actual)
        {
            throw new ArgumentException($"{argumentName} expected value: {expected}, actual value: {actual}", argumentName);
        }
    }

    public static void Equal(long expected, long actual, [CallerArgumentExpression(nameof(actual))] string? argumentName = null)
    {
        if (expected != actual)
        {
            throw new ArgumentException($"{argumentName} expected value: {expected}, actual value: {actual}", argumentName);
        }
    }

    public static void Equal(bool expected, bool actual, [CallerArgumentExpression(nameof(actual))] string? argumentName = null)
    {
        if (expected != actual)
        {
            throw new ArgumentException($"{argumentName} expected value: {expected}, actual value: {actual}", argumentName);
        }
    }

    public static void Valid<T>(T value, IValidator<T>? validator)
    {
        validator?.Validate(value);
    }
}
