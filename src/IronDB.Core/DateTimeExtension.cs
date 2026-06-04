namespace IronDB.Core;

internal static class DateTimeExtension
{
    public static DateTime Add(this DateTime date, TimeValue time)
    {
        if (time.Value == 0)
        {
            return date;
        }
        if (time.Value == int.MaxValue)
        {
            return DateTime.MaxValue;
        }
        if (time.Value == int.MinValue)
        {
            return DateTime.MinValue;
        }

        return time.Unit switch
        {
            TimeValueUnit.Month => date.AddMonths(time.Value),
            TimeValueUnit.Second => date.AddSeconds(time.Value),
            TimeValueUnit.None => date,
            _ => throw new ArgumentOutOfRangeException(nameof(time.Unit), $"Not supported time value unit '{time.Unit}'"),
        };
    }

    public static int TotalMonths(this DateTime date)
    {
        var years = date.Year;
        var months = date.Month;
        return years * 12 + months;
    }

    public static DateTime EnsureMilliseconds(this DateTime date)
    {
        var remainder = date.Ticks % 10_000;
        if (remainder != 0)
        {
            date = date.AddTicks(-remainder);
        }

        return date;
    }

    public static DateTime EnsureUtc(this DateTime date)
    {
        if (date.Kind != DateTimeKind.Local)
        {
            return date;
        }

        return date.ToUniversalTime();
    }
}