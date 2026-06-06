using IronDB.Core.LowMemory;
using System.Text;

namespace IronDB.Core.Utils;

internal static class MemoryUtils
{
    private const string GenericOutMemoryException = "Failed to generate an out of memory exception";
    private static readonly InvertedComparer InvertedComparerInstance = new();
    private const int MinAllocatedThresholdInBytes = 10 * 1024 * 1024;

    public static string GetExtendedMemoryInfo(MemoryInfoResult memoryInfo, DirtyMemoryState dirtyMemoryState)
    {
        try
        {
            var sb = new StringBuilder();
            TryAppend(() => sb.Append("Commit charge: ").Append(memoryInfo.CurrentCommitCharge).Append(" / ").Append(memoryInfo.TotalCommittableMemory).Append(", "));
            TryAppend(() => sb.Append("Memory: ").Append(memoryInfo.TotalPhysicalMemory - memoryInfo.AvailableMemory).Append(" / ").Append(memoryInfo.TotalPhysicalMemory).Append(", "));
            TryAppend(() => sb.Append("Available memory for processing: ").Append(memoryInfo.AvailableMemoryForProcessing).Append(", "));
            TryAppend(() => sb.Append("Dirty memory: ").Append(dirtyMemoryState.TotalDirty).Append(", "));
            TryAppend(() => sb.Append("Managed memory: ").Append(new Size(AbstractLowMemoryMonitor.GetManagedMemoryInBytes(), SizeUnit.Bytes)).Append(", "));
            TryAppend(() => sb.Append("Unmanaged allocations: ").Append(new Size(AbstractLowMemoryMonitor.GetUnmanagedAllocationsInBytes(), SizeUnit.Bytes)).Append(", "));
            TryAppend(() => sb.Append("Lucene managed: ").Append(new Size(NativeMemory.TotalLuceneManagedAllocationsForTermCache, SizeUnit.Bytes)).Append(", "));
            TryAppend(() => sb.Append("Lucene unmanaged term cache: ").Append(new Size(NativeMemory.TotalLuceneUnmanagedAllocationsForTermCache, SizeUnit.Bytes)).Append(", "));
            TryAppend(() => sb.Append("Lucene unmanaged sorted terms: ").Append(new Size(NativeMemory.TotalLuceneUnmanagedAllocationsForSorting, SizeUnit.Bytes)));

            var sorted = new SortedDictionary<long, (string? ThreadName, int ManagedThreadId)>(InvertedComparerInstance);

            long totalAllocatedForUnknownThreads = 0;
            int unknownThreadsCount = 0;

            foreach (var stats in NativeMemory.AllThreadStats)
            {
                if (stats.Name is null)
                {
                    totalAllocatedForUnknownThreads += stats.TotalAllocated;
                    unknownThreadsCount++;
                    continue;
                }

                sorted[stats.TotalAllocated] = (stats.Name, stats.ManagedThreadId);
            }

            sorted[totalAllocatedForUnknownThreads] = (null, 0);

            int count = 0;
            bool first = true;

            foreach (var keyValue in sorted)
            {
                if (keyValue.Key < MinAllocatedThresholdInBytes)
                    break;

                if (++count > 5)
                {
                    break;
                }

                if (first)
                {
                    first = false;
                    TryAppend(() => sb.Append(", Top threads by unmanaged allocations: "));
                }
                else
                {
                    TryAppend(() => sb.Append("; "));
                }

                sb.Append("[#");
                sb.Append(count);
                sb.Append("] ");

                TryAppend(() => sb.Append("name: ")
                    .Append(keyValue.Value.ThreadName).Append(", allocations: ")
                    .Append(new Size(keyValue.Key, SizeUnit.Bytes)));

                if (keyValue.Value.ManagedThreadId != 0)
                {
                    TryAppend(() => sb.Append(", managed thread id: ").Append(keyValue.Value.ManagedThreadId));
                }

                if (string.IsNullOrWhiteSpace(keyValue.Value.ThreadName))
                {
                    TryAppend(() => sb.Append(" (threads count: ").Append(unknownThreadsCount).Append(')'));
                }
            }

            return sb.ToString();
        }
        catch
        {
            return GenericOutMemoryException;
        }
    }

    private static void TryAppend(Action append)
    {
        try
        {
            append();
        }
        catch (Exception)
        {
            // nothing we can do here, just skip this info
        }
    }

    private sealed class InvertedComparer : IComparer<long>
    {
        public int Compare(long x, long y)
        {
            return y.CompareTo(x);
        }
    }
}
