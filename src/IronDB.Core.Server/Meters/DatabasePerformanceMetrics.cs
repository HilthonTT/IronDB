namespace IronDB.Core.Server.Meters;

public sealed class DatabasePerformanceMetrics(
    DatabasePerformanceMetrics.MetricType type, 
    int currentBufferSize, 
    int summaryBufferSize)
{
    public enum MetricType
    {
        Transaction,
        GeneralWait,
    }

    private readonly PerformanceMetrics _buffer = type switch
    {
        MetricType.Transaction => new TransactionPerformanceMetrics(currentBufferSize, summaryBufferSize),
        MetricType.GeneralWait =>new GeneralWaitPerformanceMetrics(currentBufferSize, summaryBufferSize),
        _ => throw new ArgumentException("Invalid metric type passed to DatabasePerfomanceMetrics " + type),
    };

    public PerformanceMetrics Buffer => _buffer;

    public PerformanceMetrics.DurationMeasurement MeterPerformanceRate()
    {
        return new PerformanceMetrics.DurationMeasurement(_buffer);
    }
}
