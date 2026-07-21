using IronDB.Core.Server.Logging;
using IronDB.Core.Server.Meters;
using IronDB.Core.Server.Settings;
using IronDB.Core.Utils;
using IronDB.StorageEngine.Impl.Paging;
using System.Runtime.ExceptionServices;

namespace IronDB.StorageEngine;

public abstract class StorageEnvironmentOptions : IDisposable
{
    public const string RecyclableJournalFileNamePrefix = "recyclable-journal";

    public readonly LoggingResource? LoggingResource;

    public readonly LoggingComponent? LoggingComponent;

    public IronPathSetting? TempPath { get; }

    public IronPathSetting? JournalPath { get; private set; }

    public IoMetrics? IoMetrics { get; set; }

    public bool GenerateNewDatabaseId { get; set; }

    public bool DiscardVirtualMemory { get; set; }

    public abstract AbstractPager DataPager { get; }

    public LazyWithExceptionRetry<DriveInfoByPath>? DriveInfoByPath { get; private set; }

    public void SetCatastrophicFailure(ExceptionDispatchInfo exception)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
