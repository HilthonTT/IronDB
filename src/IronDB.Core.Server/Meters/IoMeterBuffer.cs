namespace IronDB.Core.Server.Meters;

public sealed class IoMeterBuffer
{
    public sealed class MeterItem
    {
        public long Size;
        public long FileSize;
        public DateTime Start;
        public DateTime End;
        public IoMetrics.MeterType Type;
        public int Acceleration;
        public long CompressedSize;
        public TimeSpan Duration => End - Start;
        public double SizeInMb => Size / (double)1024 / 1024;
        public double RateOfWritesInMbPerSec => SizeInMb / Duration.TotalSeconds;
    }
}
