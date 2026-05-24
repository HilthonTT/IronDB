namespace IronDB.Transport.Tcp;

public sealed class TcpStats
{
    public TcpStats(
        int connections,
        long sentBytesTotal,
        long receivedBytesTotal,
        long sentBytesSinceLastRun,
        long receivedBytesSinceLastRun,
        long pendingSend,
        long inSend,
        long pendingReceived,
        TimeSpan measureTime)
    {
        Connections = connections;
        SentBytesTotal = sentBytesTotal;
        ReceivedBytesTotal = receivedBytesTotal;
        SentBytesSinceLastRun = sentBytesSinceLastRun;
        ReceivedBytesSinceLastRun = receivedBytesSinceLastRun;
        PendingSend = pendingSend;
        InSend = inSend;
        PendingReceived = pendingReceived;
        MeasureTime = measureTime;
        SendingSpeed = measureTime.TotalSeconds < 0.00001
            ? 0
            : SentBytesSinceLastRun / measureTime.TotalSeconds;
        ReceivingSpeed = measureTime.TotalSeconds < 0.00001
            ? 0
            : ReceivedBytesSinceLastRun / measureTime.TotalSeconds;
    }

    /// <summary>Number of active TCP connections.</summary>
    public int Connections { get; }

    /// <summary>Total bytes sent by TCP connections.</summary>
    public long SentBytesTotal { get; }

    /// <summary>Total bytes received from TCP connections.</summary>
    public long ReceivedBytesTotal { get; }

    /// <summary>Total bytes sent to TCP connections since last run.</summary>
    public long SentBytesSinceLastRun { get; }

    /// <summary>Total bytes received by TCP connections since last run.</summary>
    public long ReceivedBytesSinceLastRun { get; }

    /// <summary>Sending speed in bytes per second.</summary>
    public double SendingSpeed { get; }

    /// <summary>Receiving speed in bytes per second.</summary>
    public double ReceivingSpeed { get; }

    /// <summary>Number of bytes waiting to be sent to connections.</summary>
    public double PendingSend { get; }

    /// <summary>Number of bytes sent to connections but not yet acknowledged by the receiving party.</summary>
    public long InSend { get; }

    /// <summary>Number of bytes waiting to be received by connections.</summary>
    public long PendingReceived { get; }

    /// <summary>Time elapsed since last stats read.</summary>
    public TimeSpan MeasureTime { get; }
}
