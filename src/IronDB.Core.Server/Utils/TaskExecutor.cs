using IronDB.Core.Logging;
using IronDB.Core.Server.Logging;
using IronDB.Core.Utils;
using System.Collections.Concurrent;

namespace IronDB.Core.Server.Utils;

public static class TaskExecutor
{
    private static readonly IronLogger Logger = IronLogManager.Instance.GetLoggerForIronServer(typeof(TaskExecutor));
    private static readonly Runner Instance = new();

    private sealed class Runner
    {
        private const string TasksExecutorThreadName = "Task Executor";
        private readonly ConcurrentQueue<(WaitCallback, object)> _actions = new();

        private readonly ManualResetEventSlim _event = new(false);

        private void Run()
        {
            NativeMemory.EnsureRegistered();

            int tries = 0;
            while (true)
            {
                while (_actions.TryDequeue(out (WaitCallback callback, object state) result))
                {
                    try
                    {
                        result.callback(result.state);
                    }
                    catch (Exception)
                    {
                        // there is nothing that we _can_ do here that would be right
                        // and there is no meaningful error handling. Ignoring this because
                        // callers are expected to do their own exception catching
                    }
                }

                // PERF: Entering a kernel lock even if the ManualResetEventSlim will try to avoid that doing some spin locking
                //       is very costly. This is a hack that is allowing amortize a bit very high frequency events. The proper
                //       way to handle requires infrastructure changes. https://issues.hibernatingrhinos.com/issue/RavenDB-8126
                if (tries < 5)
                {
                    // Yield execution quantum. If we are in a high-frequency event we will be able to avoid the kernel lock. 
                    Thread.Sleep(0);
                    tries++;
                }
                else
                {
                    _event.WaitHandle.WaitOne();
                    _event.Reset();

                    // Nothing we can do here, just block.
                    tries = 0;
                }
            }
        }

        public void Enqueue(WaitCallback callback, object state)
        {
            _actions.Enqueue((callback, state));
            _event.Set();
        }

        public Runner()
        {
            new Thread(Run)
            {
                IsBackground = true,
                Name = TasksExecutorThreadName
            }.Start();
        }
    }

    private sealed class RunOnce(WaitCallback? callback)
    {
        private WaitCallback? _callback = callback;

        public void Execute(object? state)
        {
            var callback = _callback;
            if (callback is null)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _callback, null, callback) != callback)
            {
                return;
            }

            try
            {
                callback(state);
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                {
                    Logger.Error("Failed to execute task", e);
                }
            }
        }
    }

    public static void CompleteAndReplace(ref TaskCompletionSource<object> task)
    {
        var task2 = Interlocked.Exchange(
            ref task, 
            new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously));

        task2.TrySetResult(null!);
    }

    public static void CompleteReplaceAndExecute(ref TaskCompletionSource<object> task, Action act)
    {
        var task2 = Interlocked.Exchange(
            ref task, 
            new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously));

        Execute(state =>
        {
            if (state is null)
            {
                return;
            }

            var (tcs, action) = ((TaskCompletionSource<object>, Action))state;
            tcs.TrySetResult(null!);
            act();
        }, (task2, act));
    }

    public static void Complete(TaskCompletionSource<object> task)
    {
        task.TrySetResult(null!);
    }

    public static void Execute(WaitCallback? callback, object state)
    {
        callback = new RunOnce(callback).Execute;
        Instance.Enqueue(callback, state);
        ThreadPool.QueueUserWorkItem(callback, state);
    }
}
