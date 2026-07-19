using System.Diagnostics;

namespace IronDB.Core.Utils;

internal sealed class FifoSemaphore
{
    internal readonly Queue<OneTimeWaiter> _waitQueue;

    private readonly Lock _lock;

    private int _tokens;

    public FifoSemaphore(int tokens)
    {
        if (tokens <= 0)
        {
            throw new ArgumentException(null, nameof(tokens));
        }

        _tokens = tokens;
        _lock = new Lock();
        _waitQueue = new Queue<OneTimeWaiter>();
    }

    public bool TryAcquire(TimeSpan timeout, CancellationToken cancellationToken)
    {
        OneTimeWaiter waiter;

        lock (_lock)
        {
            if (_tokens > 0)
            {
                _tokens--;
                return true;
            }

            cancellationToken.ThrowIfCancellationRequested();

            _forTestingPurposes?.JustBeforeAddingToWaitQueue?.Invoke();

            waiter = new OneTimeWaiter(cancellationToken);

            _waitQueue.Enqueue(waiter);
        }

        using (waiter)
        {
            bool result = waiter.TryWait(timeout);

            if (!result)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }
    }

    public void Acquire(CancellationToken token)
    {
        var result = TryAcquire(Timeout.InfiniteTimeSpan, token);

        if (!result)
        {
            throw new InvalidOperationException("Could not acquire the lock");
        }
    }

    public void Release()
    {
        ReleaseMany(1);
    }

    public void ReleaseMany(int tokens)
    {
        lock (_lock)
        {
            for (int i = 0; i < tokens; i++)
            {
                if (_waitQueue.Count > 0)
                {
                    var waiter = _waitQueue.Dequeue();

                    if (!waiter.TryRelease())
                    {
                        Debug.Assert(waiter.IsCancelled || waiter.IsTimedOut, $"waiter.IsCancelled: {waiter.IsCancelled} || waiter.IsTimedOut: {waiter.IsTimedOut}");

                        // waiter was cancelled or it timed out, let's release another waiter
                        i--;
                    }
                }
                else
                {
                    // We've got no one waiting, so add a token
                    _tokens++;
                }
            }
        }
    }

    internal sealed class OneTimeWaiter(CancellationToken token) : IDisposable
    {
        private readonly ManualResetEventSlim _mre = new(false);
        private readonly CancellationToken _token = token;
        private readonly object _mreAccessLock = new ManualResetEventSlim();
        private bool _timedOut;

        public bool TryWait(TimeSpan timeout)
        {
            var indexOfSatisfiedWait = WaitHandle.WaitAny([_mre.WaitHandle, _token.WaitHandle], timeout);

            if (indexOfSatisfiedWait == 0)
            {
                return true;
            }

            lock (_mreAccessLock)
            {
                if (_mre.IsSet) // someone already managed to call Release() on it, let's ignore the already cancelled token or timeout and let it continue so it will Release
                {
                    return true;
                }

                if (indexOfSatisfiedWait == WaitHandle.WaitTimeout)
                {
                    _timedOut = true;
                }
            }

            return false;
        }

        public bool TryRelease()
        {
            lock (_mreAccessLock)
            {
                if (_token.IsCancellationRequested)
                {
                    return false;
                }

                if (_timedOut)
                {
                    return false;
                }

                _mre.Set();
                return true;
            }
        }

        public bool IsCancelled => _token.IsCancellationRequested;

        public bool IsTimedOut => _timedOut;

        public void Dispose()
        {
            _mre?.Dispose();
        }
    }

    private TestingStuff? _forTestingPurposes;

    internal TestingStuff ForTestingPurposesOnly()
    {
        if (_forTestingPurposes is not null)
        {
            return _forTestingPurposes;
        }

        return _forTestingPurposes = new TestingStuff();
    }

    internal sealed class TestingStuff
    {
        internal Action? JustBeforeAddingToWaitQueue { get; set; }
    }
}
