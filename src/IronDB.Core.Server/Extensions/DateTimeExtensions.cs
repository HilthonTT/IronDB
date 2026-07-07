using IronDB.Core.Server.Unmanaged;

namespace IronDB.Core.Server.Extensions;

public static class DateTimeExtensions
{
    public static unsafe ByteStringContext.InternalScope GetDefaultFormat(
        this DateTime dt,
        ByteStringContext context,
        out ByteString value, 
        bool isUtc)
    {
        Core.Extensions.DateTimeExtensions.ValidateDate(dt, isUtc);

        int size = 27 + (isUtc ? 1 : 0);
        long ticks = dt.Ticks;

        var scope = context.Allocate(size, out value);

        byte* ptr = value.Ptr;
        Core.Extensions.DateTimeExtensions.ProcessDefaultIronFormat(ticks, ptr);
        ptr[size - 1] = (byte)'Z';

        return scope;
    }

    public static DateTime Max(DateTime dt1, DateTime dt2) => dt1 > dt2 ? dt1 : dt2;
}
