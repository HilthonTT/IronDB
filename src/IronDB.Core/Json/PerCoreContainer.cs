using IronDB.Core.Utils;
using System.Collections;
using System.Runtime.InteropServices;

namespace IronDB.Core.Json;

public sealed class PerCoreContainer<T> : IEnumerable<T>
    where T : class
{
    private const int DefaultCapacityPerCore = 64;

    private readonly int _numberOfSlotsPerCore;
    private readonly T[][] _perCoreArrays;
    private readonly PaddedInt[] _perCoreArrayLength;

    public PerCoreContainer(int capacityPerCore = DefaultCapacityPerCore)
    {
        _numberOfSlotsPerCore = capacityPerCore;
        var numberOfCores = Environment.ProcessorCount;
        _perCoreArrays = new T[numberOfCores][];
        _perCoreArrayLength = new PaddedInt[numberOfCores];
        for (var i = 0; i < numberOfCores; i++)
        {
            _perCoreArrays[i] = new T[capacityPerCore];
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int gi = 0; gi < _perCoreArrays.Length; gi++)
        {
            T[] array = _perCoreArrays[gi];
            for (var li = 0; li < array.Length; li++)
            {
                var copy = array[li];
                if (copy is null)
                {
                    continue;
                }
                if (Interlocked.CompareExchange(ref array[li]!, null, copy) != copy)
                {
                    continue;
                }

                Interlocked.Decrement(ref _perCoreArrayLength[gi].Value);
                yield return copy;
            }
        }
    }

    public bool TryPull(out T? value)
    {
        int currentProcessorId = CurrentProcessorIdHelper.GetCurrentProcessorId() % _perCoreArrays.Length;
        if (_perCoreArrayLength[currentProcessorId].Value <= 0)
        {
            value = default;
            return false;
        }

        var coreItems = _perCoreArrays[currentProcessorId];

        for (var i = 0; i < coreItems.Length; i++)
        {
            var copy = coreItems[i];
            if (copy is null)
            {
                continue;
            }
            if (Interlocked.CompareExchange(ref coreItems[i]!, null, copy) != copy)
            {
                continue;
            }
            Interlocked.Decrement(ref _perCoreArrayLength[currentProcessorId].Value);
            value = copy;
            return true;
        }

        value = default;
        return false;
    }

    public bool TryPush(T cur)
    {
        int currentProcessorId = CurrentProcessorIdHelper.GetCurrentProcessorId() % _perCoreArrays.Length;
        if (_perCoreArrayLength[currentProcessorId].Value >= _numberOfSlotsPerCore)
        {
            return false;
        }

        var core = _perCoreArrays[currentProcessorId];

        for (int i = 0; i < core.Length; i++)
        {
            if (core[i] is not null)
            {
                continue;
            }

            if (Interlocked.CompareExchange(ref core[i], cur, null) == null)
            {
                Interlocked.Increment(ref _perCoreArrayLength[currentProcessorId].Value);
                return true;
            }
        }
        return false;
    }

    public IEnumerable<T> EnumerateAndClear()
    {
        for (var gi = 0; gi < _perCoreArrays.Length; gi++)
        {
            T[] array = _perCoreArrays[gi];
            for (int li = 0; li < array.Length; li++)
            {
                var copy = array[li];
                if (copy is null)
                {
                    continue;
                }
                if (Interlocked.CompareExchange(ref array[li]!, null, copy) != copy)
                {
                    continue;
                }

                Interlocked.Decrement(ref _perCoreArrayLength[gi].Value);
                yield return copy;
            }
        }
    }

    public bool Remove(T item, (int, int) pos)
    {
        T[]? array = _perCoreArrays[pos.Item1];
        if (array is null)
        {
            return false;
        }

        if (Interlocked.CompareExchange(ref array[pos.Item1]!, null, item) == item)
        {
            Interlocked.Decrement(ref _perCoreArrayLength[pos.Item1].Value);
            return true;
        }
        
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal struct PaddedInt
    {
        [FieldOffset(0)]
        public int Value;
    }
}
