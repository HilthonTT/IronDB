using IronDB.Core.Logging;
using IronDB.Core.Server.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace IronDB.Core.Server.Utils.DiskStatsGetter;

[SupportedOSPlatform("windows")]
internal sealed class WindowsDiskStatsGetter(TimeSpan minInterval) : DiskStatsGetter<WindowsDiskStatsRawResult>(minInterval)
{
    private static readonly IronLogger Logger = IronLogManager.Instance.GetLoggerForIronServer<WindowsDiskStatsGetter>();

    private const string DiskCategory = "LogicalDisk";

    private readonly CountersPerDisk _countersPerDisk = new();

    protected override DiskStatsResult CalculateStats(
        WindowsDiskStatsRawResult currentInfo, 
        State state)
    {
        return new DiskStatsResult
        {
            IoReadOperations = ComputeCounterValue(state.Result.RawSampling.IoReadOperations, currentInfo.IoReadOperations),
            IoWriteOperations = ComputeCounterValue(state.Result.RawSampling.IoWriteOperations, currentInfo.IoWriteOperations),
            ReadThroughput = new Size((long)ComputeCounterValue(state.Result.RawSampling.ReadThroughput, currentInfo.ReadThroughput), SizeUnit.Bytes),
            WriteThroughput = new Size((long)ComputeCounterValue(state.Result.RawSampling.WriteThroughput, currentInfo.WriteThroughput), SizeUnit.Bytes),
            QueueLength = currentInfo.QueueLength.RawValue
        };
    }

    //It should return the equivalent result like CounterSampleCalculator.ComputeCounterValue
    private static double ComputeCounterValue(CounterSample oldSample, CounterSample newSample)
    {
        var diffTime = newSample.TimeStamp - oldSample.TimeStamp;
        var diffValue = newSample.RawValue - oldSample.RawValue;
        return diffTime != 0
            ? diffValue / (diffTime / 10000000.0)
            : diffValue > 0
                ? double.PositiveInfinity
                : double.NegativeInfinity;
    }

    protected override WindowsDiskStatsRawResult? GetDiskInfo(string path)
    {
        try
        {
            DiskCounters? counters = _countersPerDisk.Get(path);
            if (counters is null)
            {
                return null;
            }

            return new WindowsDiskStatsRawResult
            {
                IoReadOperations = counters.ReadIOCounter.NextSample(),
                IoWriteOperations = counters.WriteIOCounter.NextSample(),
                ReadThroughput = counters.ReadThroughput.NextSample(),
                WriteThroughput = counters.WriteThroughput.NextSample(),
                QueueLength = counters.DiskQueue.NextSample(),
                Time = DateTime.UtcNow
            };
        }
        catch (Exception e)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn($"Could not get GetDiskInfo for {path}", e);
            }
            return null;
        }
    }

    public override void Dispose() => _countersPerDisk.Dispose();

    private sealed class DiskCounters(string drive) : IDisposable
    {
        public PerformanceCounter ReadIOCounter { get; } = new(DiskCategory, "Disk Reads/sec", drive);

        public PerformanceCounter WriteIOCounter { get; } = new(DiskCategory, "Disk Writes/sec", drive);

        public PerformanceCounter ReadThroughput { get; } = new(DiskCategory, "Disk Read Bytes/sec", drive);

        public PerformanceCounter WriteThroughput { get; } = new(DiskCategory, "Disk Write Bytes/sec", drive);

        public PerformanceCounter DiskQueue { get; } = new(DiskCategory, "Current Disk Queue Length", drive);

        public void Dispose()
        {
            ReadIOCounter?.Dispose();
            WriteIOCounter?.Dispose();
            ReadThroughput?.Dispose();
            WriteThroughput?.Dispose();
            DiskQueue?.Dispose();
        }
    }

    private class CountersPerDisk : IDisposable
    {
        private readonly PerformanceCounterCategory _category = new(DiskCategory);
        private readonly ConcurrentDictionary<string, Lazy<DiskCounters?>?> _countersPerDisk = new();

        public DiskCounters? Get(string path)
        {
            string? drive = DiskUtils.WindowsGetDriveName(path, out _);
            if (string.IsNullOrWhiteSpace(drive))
            {
                return null;
            }

            if (!_countersPerDisk.TryGetValue(drive, out var counter))
            {
                foreach (string name in _category.GetInstanceNames())
                {
                    //The return value from GetInstanceNames for example can be "C:" while the return value from WindowsGetDriveName is "C:\"
                    if (!drive.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }   

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug($"{nameof(DiskCounters)} was created for \"{drive}\" (requested for path \"{path}\").");
                    }

                    counter = _countersPerDisk.GetOrAdd(path, new Lazy<DiskCounters?>(() =>
                    {
                        try
                        {
                            return new DiskCounters(name);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            if (Logger.IsWarnEnabled)
                            {
                                Logger.Warn($"Couldn't create disk counters instance in {DiskCategory} for \"{drive}\" (requested for path \"{path}\").");
                            }

                            _countersPerDisk[path] = null;
                            return null;
                        }
                    }));

                    return counter?.Value;
                }

                if (Logger.IsWarnEnabled)
                {
                    Logger.Warn($"Couldn't find instance in {DiskCategory} for \"{drive}\" (requested for path \"{path}\").");
                }

                _countersPerDisk[path] = null;
            }

            return counter?.Value;
        }

        public void Dispose()
        {
            foreach (var (_, value) in _countersPerDisk)
            {
                value?.Value?.Dispose();
            }
        }
    }
}
