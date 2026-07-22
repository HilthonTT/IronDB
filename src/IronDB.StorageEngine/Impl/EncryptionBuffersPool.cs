using IronDB.Core.Binary;
using IronDB.Core.Json;
using IronDB.Core.Logging;
using IronDB.Core.LowMemory;
using IronDB.Core.Platform;
using IronDB.Core.Server.Logging;
using IronDB.Core.Server.Platform;
using IronDB.Core.Threading;
using IronDB.Core.Utils;
using IronDB.StorageEngine.Impl.Paging;
using IronDB.StorageEngine.Logging;
using System.Diagnostics;

namespace IronDB.StorageEngine.Impl;

public sealed unsafe class EncryptionBuffersPool : ILowMemoryHandler
{
    private readonly object _locker = new();

    public readonly static EncryptionBuffersPool Instance = new();
    private static readonly IronLogger Logger = IronLogManager.Instance.GetLoggerForGlobalEngine<EncryptionBuffersPool>();
    private const int MaxNumberOfPagesToCache = 128; // 128 * 8K = 1 MB, beyond that, we'll not both
    private readonly MultipleUseFlag _isLowMemory = new();
    private readonly MultipleUseFlag _isExtremelyLowMemory = new();
    private readonly PerCoreContainer<NativeAllocation>[] _items;
    private readonly CountingConcurrentStack<NativeAllocation>[] _globalStacks;
    private readonly Timer? _cleanupTimer;
    private long _generation;
    public bool Disabled;
    private long _currentlyInUseBytes;

    public long Generation => _generation;

    private readonly int _maxNumberOfAllocationsToKeepInGlobalStackPerSlot;
    private readonly long[] _numberOfAllocationsDisposedInGlobalStacks;

    private readonly DateTime[] _lastPerCoreCleanups;
    private readonly TimeSpan _perCoreCleanupInterval = TimeSpan.FromMinutes(5);

    private readonly DateTime[] _lastGlobalStackRebuilds;
    private readonly TimeSpan _globalStackRebuildInterval = TimeSpan.FromMinutes(15);

    public EncryptionBuffersPool(bool registerLowMemory = true, bool registerCleanup = true)
    {
        _maxNumberOfAllocationsToKeepInGlobalStackPerSlot = !PlatformDetails.Is32Bits
            ? 128
            : 32;

        var numberOfSlots = Bits.MostSignificantBit(MaxNumberOfPagesToCache * Constants.Storage.PageSize) + 1;
        _items = new PerCoreContainer<NativeAllocation>[numberOfSlots];
        _globalStacks = new CountingConcurrentStack<NativeAllocation>[numberOfSlots];
        _lastPerCoreCleanups = new DateTime[numberOfSlots];
        _lastGlobalStackRebuilds = new DateTime[numberOfSlots];
        _numberOfAllocationsDisposedInGlobalStacks = new long[numberOfSlots];

        var now = DateTime.UtcNow;

        for (int i = 0; i < _items.Length; i++)
        {
            _items[i] = new PerCoreContainer<NativeAllocation>();
            _globalStacks[i] = new CountingConcurrentStack<NativeAllocation>();
            _lastPerCoreCleanups[i] = now;
            _lastGlobalStackRebuilds[i] = now;
        }

        if (registerLowMemory)
        {
            LowMemoryNotification.Instance.RegisterLowMemoryHandler(this);
        }

        if (registerCleanup)
        {
            _cleanupTimer = new Timer(Cleanup, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
    }

    public byte* Get(int numberOfPages, out long size, out NativeMemory.ThreadStats? thread)
    {
        return Get(null, numberOfPages, out size, out thread);
    }

    public byte* Get(CryptoPager? pager, int numberOfPages, out long size, out NativeMemory.ThreadStats? thread)
    {
        var numberOfPagesPowerOfTwo = Bits.PowerOf2(numberOfPages);

        size = numberOfPagesPowerOfTwo * Constants.Storage.PageSize;

        if (Disabled || numberOfPagesPowerOfTwo > MaxNumberOfPagesToCache)
        {
            // We don't want to pool large buffers
            size = numberOfPages * Constants.Storage.PageSize;
            Interlocked.Add(ref _currentlyInUseBytes, size);

            return PlatformSpecific.NativeMemory.Allocate4KbAlignedMemory(size, out thread);
        }

        Interlocked.Add(ref _currentlyInUseBytes, size);

        var index = Bits.MostSignificantBit(size);
        NativeAllocation? allocation;
        while (_items[index].TryPull(out allocation))
        {
            if (allocation?.InUse.Raise() == false)
            {
                continue;
            }

            thread = NativeMemory.ThreadAllocations.Value;
            thread?.Allocations += size;

            Debug.Assert(size == allocation?.Size, $"size ({size}) == allocation.Size ({allocation.Size})");
#if VALIDATE
                pager?.UnprotectPageRange(allocation.Ptr, (ulong)size);
#endif
            return allocation.Ptr;
        }

        var currentGlobalStack = _globalStacks[index];

        while (currentGlobalStack.TryPop(out allocation))
        {
            if (allocation?.InUse.Raise() == false)
            {
                continue;
            }

            Debug.Assert(size == allocation?.Size, $"size ({size}) == allocation.Size ({allocation.Size})");

            thread = NativeMemory.ThreadAllocations.Value;
            thread?.Allocations += size;
#if VALIDATE
                pager?.UnprotectPageRange(allocation.Ptr, (ulong)size);
#endif
            return allocation.Ptr;
        }

        return PlatformSpecific.NativeMemory.Allocate4KbAlignedMemory(size, out thread);
    }

    public void Return(byte* ptr, long size, NativeMemory.ThreadStats allocatingThread, long generation)
    {
        Return(null, ptr, size, allocatingThread, generation);
    }

    public void Return(CryptoPager? pager, byte* ptr, long size, NativeMemory.ThreadStats? allocatingThread, long generation)
    {
        if (ptr is null)
        {
            return;
        }

        Interlocked.Add(ref _currentlyInUseBytes, -size);

        Sodium.sodium_memzero(ptr, (UIntPtr)size);

#if VALIDATE
            pager?.ProtectPageRange(ptr, (ulong)size);
#endif

        var numberOfPages = size / Constants.Storage.PageSize;

        if (Disabled || numberOfPages > MaxNumberOfPagesToCache || (_isLowMemory.IsRaised() && generation < Generation))
        {
            // - don't want to pool large buffers
            // - release all the buffers that were created before we got the low memory event
            ForTestingPurposes?.OnFree4KbAlignedMemory?.Invoke(size);
            PlatformSpecific.NativeMemory.Free4KbAlignedMemory(ptr, size, allocatingThread);
            return;
        }

        var index = Bits.MostSignificantBit(size);
        var allocation = new NativeAllocation
        {
            Ptr = ptr,
            Size = size,
            InPoolSince = DateTime.UtcNow
        };

        var addToPerCorePool = ForTestingPurposes == null || ForTestingPurposes.CanAddToPerCorePool;
        var success = addToPerCorePool && _items[index].TryPush(allocation);

        if (success)
        {
            // updating the thread allocations since we released the memory back to the pool
            ForTestingPurposes?.OnUpdateMemoryStatsForThread?.Invoke(size);
            NativeMemory.UpdateMemoryStatsForThread(allocatingThread, size);
            return;
        }

        var addToGlobalPool = ForTestingPurposes == null || ForTestingPurposes.CanAddToGlobalPool;

        var currentGlobalStack = _globalStacks[index];
        if (addToGlobalPool && currentGlobalStack.Count < _maxNumberOfAllocationsToKeepInGlobalStackPerSlot)
        {
            // updating the thread allocations since we released the memory back to the pool
            ForTestingPurposes?.OnUpdateMemoryStatsForThread?.Invoke(size);
            NativeMemory.UpdateMemoryStatsForThread(allocatingThread, size);
            currentGlobalStack.Push(allocation);
            return;
        }

        ForTestingPurposes?.OnFree4KbAlignedMemory?.Invoke(size);
        PlatformSpecific.NativeMemory.Free4KbAlignedMemory(ptr, size, allocatingThread);
    }

    public void LowMemory(LowMemorySeverity lowMemorySeverity)
    {
        if (_isLowMemory.Raise())
        {
            Interlocked.Increment(ref _generation);
        }

        if (lowMemorySeverity != LowMemorySeverity.ExtremelyLow)
        {
            return;
        }

        if (!_isExtremelyLowMemory.Raise())
        {
            return;
        }

        for (int i = 0; i < _items.Length; i++)
        {
            ClearStack(_globalStacks[i]);

            foreach (var allocation in _items[i].EnumerateAndClear())
            {
                if (allocation.InUse.Raise())
                    allocation.Dispose();
            }
        }

        static void ClearStack(CountingConcurrentStack<NativeAllocation> stack)
        {
            if (stack is null || stack.IsEmpty)
            {
                return;
            }

            while (stack.TryPop(out var allocation))
            {
                if (allocation?.InUse.Raise() == true)
                {
                    allocation.Dispose();
                }
            }
        }
    }

    public void LowMemoryOver()
    {
        _isLowMemory.Lower();
        _isExtremelyLowMemory.Lower();
    }

    public EncryptionBufferStats GetStats()
    {
        var stats = new EncryptionBufferStats();
        stats.Disabled = Disabled;
        stats.CurrentlyInUseSize = _currentlyInUseBytes;

        for (int i = 0; i < _items.Length; i++)
        {
            var totalStackSize = 0L;
            var totalGlobalStackSize = 0L;

            var numberOfItems = 0;
            var numberOfGlobalStackItems = 0;

            foreach (var (allocation, _) in _items[i])
            {
                if (allocation.InUse.IsRaised())
                {
                    // not in the pool or disposed
                    continue;
                }

                totalStackSize += allocation.Size;
                numberOfItems++;
            }

            foreach (var allocation in _globalStacks[i])
            {
                if (allocation.InUse.IsRaised())
                {
                    // not in the pool or disposed
                    continue;
                }

                totalGlobalStackSize += allocation.Size;
                numberOfGlobalStackItems++;
            }

            if (numberOfItems > 0)
            {
                stats.TotalPoolSize += totalStackSize;
                stats.TotalNumberOfItems += numberOfItems;

                stats.Details.Add(new EncryptionBufferStats.AllocationInfo
                {
                    AllocationType = EncryptionBufferStats.AllocationType.PerCore,
                    TotalSize = totalStackSize,
                    NumberOfItems = numberOfItems,
                    AllocationSize = totalStackSize / numberOfItems
                });
            }

            if (numberOfGlobalStackItems > 0)
            {
                stats.TotalPoolSize += totalGlobalStackSize;
                stats.TotalNumberOfItems += numberOfGlobalStackItems;

                stats.Details.Add(new EncryptionBufferStats.AllocationInfo
                {
                    AllocationType = EncryptionBufferStats.AllocationType.Global,
                    TotalSize = totalGlobalStackSize,
                    NumberOfItems = numberOfGlobalStackItems,
                    AllocationSize = totalGlobalStackSize / numberOfGlobalStackItems
                });
            }
        }

        return stats;
    }

    private void Cleanup(object? _)
    {
        if (!Monitor.TryEnter(_locker))
        {
            return;
        }

        try
        {
            var currentTime = DateTime.UtcNow;
            var idleTime = TimeSpan.FromMinutes(10);

            for (int i = 0; i < _items.Length; i++)
            {
                var currentStack = _items[i];
                var currentGlobalStack = _globalStacks[i];

                var perCoreCleanupNeeded = currentGlobalStack.IsEmpty || currentTime - _lastPerCoreCleanups[i] >= _perCoreCleanupInterval;
                if (perCoreCleanupNeeded)
                {
                    _lastPerCoreCleanups[i] = currentTime;

                    foreach (var current in currentStack)
                    {
                        var allocation = current.Item;
                        var timeInPool = currentTime - allocation.InPoolSince;
                        if (timeInPool <= idleTime)
                            continue;

                        if (allocation.InUse.Raise() == false)
                            continue;

                        currentStack.Remove(current.Item, current.Pos);
                        allocation.Dispose();
                    }

                    continue;
                }

                using (var globalStackEnumerator = currentGlobalStack.GetEnumerator())
                {
                    while (globalStackEnumerator.MoveNext())
                    {
                        var allocation = globalStackEnumerator.Current;

                        var timeInPool = currentTime - allocation.InPoolSince;
                        if (timeInPool <= idleTime)
                            continue;

                        if (allocation.InUse.Raise() == false)
                            continue;

                        allocation.Dispose();
                        _numberOfAllocationsDisposedInGlobalStacks[i]++;
                    }
                }

                var globalStackRebuildNeeded = currentTime - _lastGlobalStackRebuilds[i] >= _globalStackRebuildInterval;

                if (globalStackRebuildNeeded && _numberOfAllocationsDisposedInGlobalStacks[i] > 0)
                {
                    _lastGlobalStackRebuilds[i] = currentTime;

                    _numberOfAllocationsDisposedInGlobalStacks[i] = 0;

                    var localStack = new CountingConcurrentStack<NativeAllocation>();

                    while (currentGlobalStack.TryPop(out var allocation))
                    {
                        if (allocation?.InUse.Raise() == false)
                        {
                            continue;
                        }

                        allocation?.InUse.Lower();
                        if (allocation is not null)
                        {
                            localStack.Push(allocation);
                        }
                    }

                    while (localStack.TryPop(out var allocation))
                    {
                        if (allocation is not null)
                        {
                            currentGlobalStack.Push(allocation);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Assert(e is OutOfMemoryException, $"Expecting OutOfMemoryException but got: {e}");
            if (Logger.IsErrorEnabled)
                Logger.Error("Error during cleanup.", e);
        }
        finally
        {
            Monitor.Exit(_locker);
        }
    }

    private sealed class NativeAllocation : PooledItem
    {
        public byte* Ptr;
        public long Size;

        public override void Dispose()
        {
            PlatformSpecific.NativeMemory.Free4KbAlignedMemory(Ptr, Size, null);
        }
    }

    internal TestingStuff? ForTestingPurposes;

    internal TestingStuff ForTestingPurposesOnly()
    {
        if (ForTestingPurposes is not null)
        {
            return ForTestingPurposes;
        }

        return ForTestingPurposes = new TestingStuff();
    }

    internal sealed class TestingStuff
    {
        public bool CanAddToPerCorePool = true;

        public bool CanAddToGlobalPool = true;

        public Action<long>? OnFree4KbAlignedMemory { get; set; } = default;

        public Action<long>? OnUpdateMemoryStatsForThread { get; set; } = default;
    }
}
