using IronDB.Core.Logging;

namespace IronDB.Core.Utils;

internal static class CancellationTokenSourceExtensions
{
    /// <summary>
    /// Cancels the <see cref="CancellationTokenSource"/> and catches any <see cref="AggregateException"/>
    /// thrown by registered callbacks (e.g. stream.Dispose registered via token.Register in ParseToMemoryAsync).
    /// CancellationTokenSource.Cancel() wraps all callback exceptions in AggregateException — it is the only exception type it throws.
    /// Catching it allows the caller's cleanup to proceed regardless of broken-connection errors in callbacks.
    /// </summary>
    public static void SafeCancel(this CancellationTokenSource ctx, IIronLogger logger, string component)
    {
        try
        {
            ctx.Cancel();
        }
        catch (ObjectDisposedException ex)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"Failed to cancel {nameof(CancellationTokenSource)} while disposing of {component}", ex);
            }
        }
    }
}
