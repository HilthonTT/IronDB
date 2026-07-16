using System.Diagnostics;

namespace IronDB.Core.Server.Utils.DiskStatsGetter;

internal sealed record WindowsDiskStatsRawResult : IDiskStatsRawResult
{
    public CounterSample IoReadOperations { get; init; }

    public CounterSample IoWriteOperations { get; init; }

    public CounterSample ReadThroughput { get; init; }

    public CounterSample WriteThroughput { get; init; }

    public CounterSample QueueLength { get; init; }

    public DateTime Time { get; init; }
}
