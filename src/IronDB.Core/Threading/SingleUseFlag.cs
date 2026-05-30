using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Threading;

public sealed class SingleUseFlag
{
    private int _state;

    public SingleUseFlag(SingleUseFlag other)
    {
        throw new InvalidOperationException($"Copy of {nameof(SingleUseFlag)} is forbidden");
    }

    /// <summary>
    /// Creates a flag.
    /// </summary>
    /// <param name="raised">if it should be raised or not</param>
    public SingleUseFlag(bool raised = false)
    {
        _state = 0;
        if (raised)
        {
            Interlocked.Exchange(ref _state, 1);
        }
    }

    /// <summary>
    /// Raises the flag. If already up, throws InvalidOperationException.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaiseOrDie()
    {
        if (Raise() == false)
            ThrowException();
    }

    /// <summary>
    /// This is here to allow RaiseOrDie() to be inlined.
    /// </summary>
    private static void ThrowException()
    {
        const string message = $"Repeated Raise for a {nameof(SingleUseFlag)} instance";
        Debug.Assert(false, message);
        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Raises the flag
    /// </summary>
    /// <returns>If already raised, false; otherwise, true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Raise()
    {
        return Interlocked.CompareExchange(ref _state, 1, 0) == 0;
    }

    /// <returns>True iff the flag is raised</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRaised()
    {
        return _state != 0;
    }

    /// <summary>
    /// Returns true iff the flag is raised. Same as calling IsRaised().
    /// </summary>
    /// <param name="flag">Flag to check</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(SingleUseFlag flag)
    {
        if (flag is null)
        {
            return false;
        }

        return flag.IsRaised();
    }
}
