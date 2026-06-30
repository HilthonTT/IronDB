using IronDB.Core.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Server;

public sealed class ContentionLoggingLocker(IIronLogger logger, string name)
{
    private readonly IIronLogger _logger = logger;
    private readonly string _name = name;
    private readonly object _locker = new();
    private bool _lockTaken = false;

    public readonly struct Release(ContentionLoggingLocker parent) : IDisposable
    {
        private readonly ContentionLoggingLocker _parent = parent;

        public void Dispose()
        {
            if (_parent._lockTaken)
            {
                Monitor.Exit(_parent._locker);
            }
        }
    }

    public Release Lock(
        [CallerMemberName] string? caller = null, 
        [CallerLineNumber] int line = 0)
    {
        _lockTaken = true;
        Monitor.TryEnter(_locker, 0, ref _lockTaken);

        if (!_lockTaken)
        {
            var sp = Stopwatch.StartNew();
            Monitor.TryEnter(_locker, Timeout.Infinite, ref _lockTaken);
            Debug.Assert(_lockTaken);
            if (_logger.IsInfoEnabled)
            {
                _logger.Info($"Contention on lock {_name} from {caller} : {line} for {sp.ElapsedMilliseconds:#,#;;0} ms");
            }
        }

        return new Release(this);
    }
}
