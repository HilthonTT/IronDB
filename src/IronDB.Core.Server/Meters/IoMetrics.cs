using System.Collections.Concurrent;
using static IronDB.Core.Server.IoChange;

namespace IronDB.Core.Server.Meters;

public sealed class IoMetrics(int currentBufferSize, int summaryBufferSize, IoChange.IoChangesNotifications? ioChanges = null)
{
    public enum MeterType
    {
        Compression,
        JournalWrite,
        DataFlush,
        DataSync,
    }

    private readonly ConcurrentDictionary<string, FileIoMetrics> _fileMetrics = new();
    private readonly ConcurrentQueue<string> _closedFiles = new();
    private readonly IoChangesNotifications? _ioChanges = ioChanges;

    public int BufferSize { get; } = currentBufferSize;
    public int SummaryBufferSize { get; } = summaryBufferSize;

    public IEnumerable<FileIoMetrics> Files => _fileMetrics.Values;

    public void FileClosed(string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return;
        }

        if (!_fileMetrics.TryGetValue(filename, out var value))
        {
            return;
        }

        value.Closed = true;
        _closedFiles.Enqueue(filename);

        while (_closedFiles.Count > 16)
        {
            if (!_closedFiles.TryDequeue(out filename))
            {
                return;
            }
            _fileMetrics.TryRemove(filename, out _);
        }
    }

    public IoMeterBuffer.DurationMeasurement MeterIoRate(string fileName, MeterType type, long size)
    {
        if (BufferSize == 0)
        {
            return default;
        }

        var fileIoMetrics = _fileMetrics.GetOrAdd(fileName, fn => new FileIoMetrics(fn, BufferSize, SummaryBufferSize));

        IoMeterBuffer buffer = type switch
        {
            MeterType.Compression => fileIoMetrics.Compression,
            MeterType.JournalWrite => fileIoMetrics.JournalWrite,
            MeterType.DataFlush => fileIoMetrics.DataFlush,
            MeterType.DataSync => fileIoMetrics.DataSync,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        void OnFileChange(IoMeterBuffer.MeterItem meterItem)
        {
            _ioChanges?.RaiseNotifications(fileName, meterItem);
        }

        return new IoMeterBuffer.DurationMeasurement(buffer, type, size, 0, OnFileChange);
    }

    public sealed class FileIoMetrics
    {
        public string FileName;
        public IoMeterBuffer JournalWrite;
        public IoMeterBuffer Compression;
        public IoMeterBuffer DataFlush;
        public IoMeterBuffer DataSync;

        public bool Closed;

        public FileIoMetrics(string filename, int metricsBufferSize, int summaryBufferSize)
        {
            FileName = filename;

            Compression = new IoMeterBuffer(metricsBufferSize, summaryBufferSize);
            JournalWrite = new IoMeterBuffer(metricsBufferSize, summaryBufferSize);
            DataFlush = new IoMeterBuffer(metricsBufferSize, summaryBufferSize);
            DataSync = new IoMeterBuffer(metricsBufferSize, summaryBufferSize);
        }

        public List<IoMeterBuffer.MeterItem> GetRecentMetrics()
        {
            var list = new List<IoMeterBuffer.MeterItem>();
            list.AddRange(Compression.GetCurrentItems());
            list.AddRange(DataSync.GetCurrentItems());
            list.AddRange(JournalWrite.GetCurrentItems());
            list.AddRange(DataFlush.GetCurrentItems());

            list.Sort((x, y) => x.Start.CompareTo(y.Start));

            return list;
        }

        public List<IoMeterBuffer.SummerizedItem> GetSummaryMetrics()
        {
            var list = new List<IoMeterBuffer.SummerizedItem>();
            list.AddRange(Compression.GetSummerizedItems());
            list.AddRange(DataSync.GetSummerizedItems());
            list.AddRange(DataFlush.GetSummerizedItems());
            list.AddRange(JournalWrite.GetSummerizedItems());

            list.Sort((x, y) => x.TotalTimeStart.CompareTo(y.TotalTimeStart));

            return list;
        }
    }
}