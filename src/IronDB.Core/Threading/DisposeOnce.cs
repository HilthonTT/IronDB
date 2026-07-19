namespace IronDB.Core.Threading;

public sealed class DisposeOnce<TOperationMode> : IDisposable
    where TOperationMode : struct, IDisposeOnceOperationMode
{
    private readonly Action _action;

    private Tuple<MultipleUseFlag, TaskCompletionSource<object>> _state = 
        Tuple.Create(
            new MultipleUseFlag(), 
            new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously));

    readonly TOperationMode _operationModeData = default;

    public DisposeOnce(Action action)
    {
        _action = action;

        if (typeof(TOperationMode) != typeof(ExceptionRetry) &&
            typeof(TOperationMode) != typeof(SingleAttempt))
        {
            throw new NotSupportedException($"Unknown operation mode: {typeof(TOperationMode).Name}");
        }

        _operationModeData.Initialize();
    }

    public bool DisposedRequested => Disposed || _operationModeData.DuringDispose;

    public bool Disposed
    {
        get
        {
            var state = _state;
            if (!state.Item1)
            {
                return false;
            }

            if (typeof(TOperationMode) == typeof(SingleAttempt))
            {
                return _operationModeData.DuringDispose == false;
            }

            if (typeof(TOperationMode) == typeof(ExceptionRetry))
            {
                if (state.Item2.Task.IsFaulted || state.Item2.Task.IsCanceled)
                {
                    return false;
                }

                return state.Item2.Task.IsCompleted;
            }

            throw new NotSupportedException($"Unknown operation mode: {typeof(TOperationMode).Name}");
        }
    }

    /// <summary>
    /// Runs the dispose action. Ensures any threads that are running it
    /// concurrently wait for the dispose to finish if it is in progress.
    /// 
    /// If the dispose has already happened, the <see cref="TOperationMode"/> defines
    /// how Dispose will react. The two approaches differ only in error
    /// handling.
    /// 
    /// When behavior is <see cref="ExceptionRetry"/>, we will retry the
    /// Dispose until it succeeds. Retry, however, happens on successive
    /// calls to Dispose, rather than in a single attempt.
    /// 
    /// When behavior is <see cref="SingleAttempt"/> or <see cref="SingleAttemptWithWaitForDisposeToFinish"/>, a failure means all
    /// subsequent calls will fail by throwing the same exception that
    /// was thrown by the action.
    /// </summary>
    public void Dispose()
    {
        _operationModeData.EnterDispose();

        try
        {
            Tuple<MultipleUseFlag, TaskCompletionSource<object>> localState = _state;
            MultipleUseFlag disposeInProgress = localState.Item1;

            if (!disposeInProgress.Raise())
            {
                // If a dispose is in progress, all other threads
                // attempting to dispose will stop here and wait for the dispose to finish.
                // Once the dispose finishes,
                // the waiting threads will check if the dispose succeeded or failed and react accordingly based on the operation mode.
                localState.Item2.Task.Wait();
                return;
            }

            try
            {
                _action();
                localState.Item2.SetResult(default!);
            }
            catch (Exception ex)
            {
                if (typeof(TOperationMode) == typeof(ExceptionRetry))
                {
                    // Reset the state for the next attempt. First backup the
                    // current task completion.
                    // Let everyone waiting know that this run failed
                    localState.Item2.SetException(ex);

                    // atomically replace both the flag and the task to wait, so new 
                    // callers to the Dispose are either getting the error or can start
                    // calling this again
                    Interlocked.CompareExchange(ref _state,
                        Tuple.Create(new MultipleUseFlag(), new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously)),
                        localState
                    );
                }
                else if (typeof(TOperationMode) == typeof(SingleAttempt))
                {
                    // Let everyone waiting know that this run failed
                    localState.Item2.SetException(ex);
                }
                else
                {
                    throw new NotSupportedException($"Unknown operation mode: {typeof(TOperationMode).Name}");
                }

                throw;
            }
        }
        finally
        {
            _operationModeData.LeaveDispose();
        }
    }
}
