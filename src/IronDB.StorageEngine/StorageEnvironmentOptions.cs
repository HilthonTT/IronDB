using IronDB.Core.Server.Logging;
using IronDB.Core.Server.Meters;
using IronDB.Core.Server.Settings;
using IronDB.Core.Utils;

namespace IronDB.StorageEngine;

public abstract class StorageEnvironmentOptions : IDisposable
{
    public const string RecyclableJournalFileNamePrefix = "recyclable-journal";


    public readonly LoggingComponent? LoggingComponent;

    public IronPathSetting? TempPath { get; }

    public IronPathSetting? JournalPath { get; private set; }

    public IoMetrics? IoMetrics { get; set; }

    public bool GenerateNewDatabaseId { get; set; }

    public bool DiscardVirtualMemory { get; set; }

    public LazyWithExceptionRetry<DriveInfoByPath>? DriveInfoByPath { get; private set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
