using IronDB.Core.Collections;
using IronDB.Core.Platform;
using IronDB.Core.Server.Logging;
using IronDB.Core.Server.Meters;
using IronDB.Core.Server.Settings;
using IronDB.Core.Utils;
using IronDB.StorageEngine.Exceptions;
using IronDB.StorageEngine.Impl;
using IronDB.StorageEngine.Impl.Journal;
using IronDB.StorageEngine.Impl.Paging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace IronDB.StorageEngine;

public abstract class StorageEnvironmentOptions : IDisposable
{
    private static bool _skipCatastrophicFailureAssertion = false;
    private readonly CatastrophicFailureNotification _catastrophicFailureNotification = default!;
    private readonly ConcurrentSet<CryptoPager> _activeCryptoPagers = [];

    private long _initialLogFileSize;
    private long _maxLogFileSize;
    private long _maxScratchBufferSize;

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

    public long PrefetchSegmentSize { get; set; }

    public long PrefetchResetThreshold { get; set; }

    public long SyncJournalsCountThreshold { get; set; }

    internal bool CopyOnWriteMode { get; set; }

    public StorageEncryptionOptions Encryption { get; } = new();

    public int PageSize = Constants.Storage.PageSize;

    public bool EnablePrefetching = true;

    public long MaxNumberOfPagesInJournalBeforeFlush { get; set; }

    public long MaxLogFileSize
    {
        get { return _maxLogFileSize; }
        set
        {
            if (value < _initialLogFileSize)
                InitialLogFileSize = value;
            _maxLogFileSize = value;
        }
    }

    public long InitialLogFileSize
    {
        get { return _initialLogFileSize; }
        set
        {
            if (value > MaxLogFileSize)
            {
                MaxLogFileSize = value;
            }
            if (value <= 0)
            {
                ThrowInitialLogFileSizeOutOfRange();
            }
            _initialLogFileSize = value;
        }
    }

    private bool _forceUsing32BitsPager;
    public bool ForceUsing32BitsPager
    {
        get => _forceUsing32BitsPager;
        set
        {
            _forceUsing32BitsPager = value;
            MaxLogFileSize = (value ? 32 : 256) * Constants.Size.Megabyte;
            MaxScratchBufferSize = (value ? 32 : 256) * Constants.Size.Megabyte;
            MaxNumberOfPagesInJournalBeforeFlush = (value ? 4 : 32) * Constants.Size.Megabyte / Constants.Storage.PageSize;
        }
    }

    public long MaxScratchBufferSize
    {
        get => _maxScratchBufferSize;
        set
        {
            if (value < 0)
                throw new InvalidOperationException($"Cannot set {nameof(MaxScratchBufferSize)} to negative value: {value}");

            if (_forceUsing32BitsPager && _maxScratchBufferSize > 0)
            {
                _maxScratchBufferSize = Math.Min(value, _maxScratchBufferSize);
                return;
            }

            _maxScratchBufferSize = value;
        }
    }

    public void TrackCryptoPager(CryptoPager cryptoPager)
    {
        _activeCryptoPagers.Add(cryptoPager);
    }

    public void UntrackCryptoPager(CryptoPager cryptoPager)
    {
        _activeCryptoPagers.TryRemove(cryptoPager);
    }

    public void SetCatastrophicFailure(ExceptionDispatchInfo exception)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Encryption.Dispose();

        GC.SuppressFinalize(this);
    }

    public sealed class StorageEncryptionOptions : IDisposable
    {
        private IJournalCompressionBufferCryptoHandler? _journalCompressionBufferHandler;

        public IJournalCompressionBufferCryptoHandler JournalCompressionBufferHandler
        {
            get
            {
                if (!HasExternalJournalCompressionBufferHandlerRegistration)
                {
                    throw new InvalidOperationException($"You have to {nameof(RegisterForJournalCompressionHandler)} before you try to access {nameof(JournalCompressionBufferHandler)}");
                }

                return _journalCompressionBufferHandler ??= default!;
            }
            private set => _journalCompressionBufferHandler = value;
        }

        public byte[]? MasterKey;

        public bool IsEnabled => MasterKey != null;

        public EncryptionBuffersPool EncryptionBuffersPool = EncryptionBuffersPool.Instance;

        public bool HasExternalJournalCompressionBufferHandlerRegistration { get; private set; }

        public void RegisterForJournalCompressionHandler()
        {
            if (IsEnabled == false)
                return;

            HasExternalJournalCompressionBufferHandlerRegistration = true;
        }

        public void SetExternalCompressionBufferHandler(IJournalCompressionBufferCryptoHandler handler)
        {
            JournalCompressionBufferHandler = handler;
        }

        public unsafe void Dispose()
        {
            var copy = MasterKey;
            if (copy != null)
            {
                fixed (byte* key = copy)
                {
                    Sodium.sodium_memzero(key, (UIntPtr)copy.Length);
                    MasterKey = null;
                }
            }
        }
    }

    internal TestingStuff? ForTestingPurposes;

    internal TestingStuff ForTestingPurposesOnly()
    {
        if (ForTestingPurposes != null)
        {
            return ForTestingPurposes;
        }

        return ForTestingPurposes = new TestingStuff();
    }

    internal sealed class TestingStuff
    {
        public int? WriteToJournalCompressionAcceleration = null;
    }

    [DoesNotReturn]
    private static void ThrowInitialLogFileSizeOutOfRange()
    {
        throw new ArgumentOutOfRangeException("InitialLogFileSize", "The initial log for the Voron must be above zero");
    }
}
