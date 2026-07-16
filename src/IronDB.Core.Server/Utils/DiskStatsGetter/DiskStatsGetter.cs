using System.Collections.Concurrent;

namespace IronDB.Core.Server.Utils.DiskStatsGetter;

internal abstract class DiskStatsGetter<T>(TimeSpan minInterval) : IDiskStatsGetter
    where T : class, IDiskStatsRawResult
{
    private readonly TimeSpan _minInterval = minInterval;
    private readonly TimeSpan _maxWait = TimeSpan.FromMilliseconds(100);

    private readonly ConcurrentDictionary<string, DiskStatsCache> _previousInfo = new();

    public DiskStatsResult? Get(string? drive)
    {
        return GetAsync(drive).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task<DiskStatsResult?> GetAsync(string? drive)
    {
        if (string.IsNullOrWhiteSpace(drive))
        {
            return null;
        }

        var start = DateTime.UtcNow;
        State? state = null;

        while (true)
        {
            if (!_previousInfo.TryGetValue(drive, out DiskStatsCache? cache))
            {
                state ??= new State 
                { 
                    Drive = drive 
                };

                var task = new Task<GetStatsResult>(GetStats, state);

                var diskStatsCache = new DiskStatsCache { Task = task };
                if (_previousInfo.TryAdd(drive, diskStatsCache))
                {
                    task.Start();
                }
                return null;
            }

            if (!cache.Task.IsCompleted)
            {
                await Task.WhenAny(cache.Task, Task.Delay(_maxWait)).ConfigureAwait(false);
                if (!cache.Task.IsCompleted)
                {
                    return cache.Value;
                }
            }

            var prevValue = cache.Task.Result;
            if (prevValue == GetStatsResult.Empty)
            {
                _previousInfo.TryRemove(new KeyValuePair<string, DiskStatsCache>(drive, cache));
                continue;
            }

            var diff = DateTime.UtcNow - prevValue.RawSampling.Time;
            if (start < prevValue.RawSampling.Time || diff < _minInterval)
            {
                return prevValue.Calculated;
            }

            state ??= new State { Drive = drive };
            state.Result = prevValue;

            var calculateTask = new Task<GetStatsResult>(CalculateStats, state);
            if (!_previousInfo.TryUpdate(drive, new DiskStatsCache { Value = prevValue.Calculated, Task = calculateTask }, cache))
            {
                continue;
            }

            calculateTask.Start();
        }
    }

    private GetStatsResult GetStats(object? obj)
    {
        if (obj is null)
        {
            return GetStatsResult.Empty;
        }

        var state = (State)obj;
        var currentInfo = GetDiskInfo(state.Drive);

        return currentInfo is null
            ? GetStatsResult.Empty
            : new GetStatsResult { RawSampling = currentInfo };
    }

    private GetStatsResult CalculateStats(object? obj)
    {
        if (obj is null)
        {
            return GetStatsResult.Empty;
        }

        var state = (State)obj;
        T? currentInfo = GetDiskInfo(state.Drive);
        if (currentInfo is null)
        {
            return GetStatsResult.Empty;
        }

        var diskSpaceResult = CalculateStats(currentInfo, state);
        return new GetStatsResult 
        {
            RawSampling = currentInfo, 
            Calculated = diskSpaceResult 
        };
    }

    protected abstract DiskStatsResult CalculateStats(T currentInfo, State state);

    protected class State
    {
        public string Drive { get; set; } = string.Empty;

        public GetStatsResult Result { get; set; } = default!;
    }

    protected abstract T? GetDiskInfo(string path);

    protected class GetStatsResult
    {
        public static GetStatsResult Empty = new();

        public DiskStatsResult Calculated { get; set; } = default!;

        public T RawSampling { get; set; } = default!;
    }

    private class DiskStatsCache
    {
        public DiskStatsResult Value { get; init; } = default!;

        public Task<GetStatsResult> Task { get; init; } = default!;
    }

    public abstract void Dispose();
}
