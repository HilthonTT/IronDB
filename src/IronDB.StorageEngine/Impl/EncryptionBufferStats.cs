using IronDB.Core;
using IronDB.Core.Json.Parsing;

namespace IronDB.StorageEngine.Impl;

public sealed class EncryptionBufferStats : IDynamicJson
{
    public EncryptionBufferStats()
    {
        Details = [];
    }

    public bool Disabled { get; set; }

    public List<AllocationInfo> Details { get; private set; }

    public long TotalPoolSize { get; set; }

    public long CurrentlyInUseSize { get; set; }

    public Size CurrentlyInUseSizeHumane => new Size(CurrentlyInUseSize, SizeUnit.Bytes);

    public Size TotalPoolSizeHumane => new Size(TotalPoolSize, SizeUnit.Bytes);

    public long TotalNumberOfItems { get; set; }

    public sealed class AllocationInfo : IDynamicJson
    {
        public AllocationType AllocationType { get; set; }

        public long TotalSize { get; set; }

        public Size TotalSizeHumane => new Size(TotalSize, SizeUnit.Bytes);

        public int NumberOfItems { get; set; }

        public long AllocationSize { get; set; }

        public Size AllocationSizeHumane => new Size(AllocationSize, SizeUnit.Bytes);

        public DynamicJsonValue ToJson()
        {
            return new DynamicJsonValue
            {
                [nameof(AllocationType)] = AllocationType,
                [nameof(NumberOfItems)] = NumberOfItems,
                [nameof(TotalSize)] = TotalSize,
                [nameof(TotalSizeHumane)] = TotalSizeHumane.ToString(),
                [nameof(AllocationSize)] = AllocationSize,
                [nameof(AllocationSizeHumane)] = AllocationSizeHumane.ToString()
            };
        }
    }

    public enum AllocationType
    {
        PerCore,
        Global
    }

    public DynamicJsonValue ToJson()
    {
        return new DynamicJsonValue
        {
            [nameof(Disabled)] = Disabled,
            [nameof(CurrentlyInUseSize)] = CurrentlyInUseSize,
            [nameof(CurrentlyInUseSizeHumane)] = CurrentlyInUseSizeHumane.ToString(),
            [nameof(TotalPoolSize)] = TotalPoolSize,
            [nameof(TotalPoolSizeHumane)] = TotalPoolSizeHumane.ToString(),
            [nameof(TotalNumberOfItems)] = TotalNumberOfItems,
            [nameof(Details)] = Details.OrderByDescending(x => x.TotalSize).Select(x => x.ToJson())
        };
    }
}
