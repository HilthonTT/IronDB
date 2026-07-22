using IronDB.StorageEngine.Impl;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace IronDB.StorageEngine.Exceptions;

public class EngineUnrecoverableErrorException : Exception
{
    [DoesNotReturn]
    public static void Raise(LowLevelTransaction tx, string message)
    {
        try
        {
            string lastTxState = tx.GetTxState();
            tx.MarkTransactionAsFailed();
            throw new EngineUnrecoverableErrorException($"{message}. LastTxState: {lastTxState}"
                + Environment.NewLine + " @ " + tx.Environment.Options.DataPager.FileName.FullPath);
        }
        catch (Exception e)
        {
            tx.Environment.Options.SetCatastrophicFailure(ExceptionDispatchInfo.Capture(e));
            throw;
        }
    }

    [DoesNotReturn]
    public static void Raise(StorageEnvironment env, string message)
    {
        try
        {
            throw new EngineUnrecoverableErrorException(message + Environment.NewLine + " @ " + env.Options.DataPager.FileName.FullPath);
        }
        catch (Exception e)
        {
            env.Options.SetCatastrophicFailure(ExceptionDispatchInfo.Capture(e));
            throw;
        }
    }

    [DoesNotReturn]
    public static void Raise(StorageEnvironmentOptions options, string message)
    {
        try
        {
            throw new EngineUnrecoverableErrorException(message
                + Environment.NewLine + " @ " + options.DataPager.FileName.FullPath);
        }
        catch (Exception e)
        {
            options.SetCatastrophicFailure(ExceptionDispatchInfo.Capture(e));
            throw;
        }
    }

    [DoesNotReturn]
    public static void Raise(StorageEnvironment env, string message, Exception inner)
    {
        try
        {
            throw new EngineUnrecoverableErrorException(message
                + Environment.NewLine + " @ " + env.Options.DataPager.FileName.FullPath, inner);
        }
        catch (Exception e)
        {
            env.Options.SetCatastrophicFailure(ExceptionDispatchInfo.Capture(e));
            throw;
        }
    }

    [DoesNotReturn]
    public static void Raise(StorageEnvironmentOptions options, string message, Exception inner)
    {
        try
        {
            throw new EngineUnrecoverableErrorException(message
                + Environment.NewLine + " @ " + options.DataPager.FileName.FullPath, inner);
        }
        catch (Exception e)
        {
            options.SetCatastrophicFailure(ExceptionDispatchInfo.Capture(e));
            throw;
        }
    }

    [DoesNotReturn]
    public static void Raise(string message, Exception inner)
    {
        throw new EngineUnrecoverableErrorException(message, inner);
    }

    protected EngineUnrecoverableErrorException(string message)
        : base(message)
    {
    }

    private EngineUnrecoverableErrorException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
