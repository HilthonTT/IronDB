using System.Runtime.CompilerServices;

namespace IronDB.Core.Threading;

public sealed class MultipleUseFlag
{
    private int _state;

    public MultipleUseFlag(MultipleUseFlag other)
    {
        throw new InvalidOperationException($"Copy of {nameof(MultipleUseFlag)} is forbidden");
    }

    /// <summary>
    /// Creates a flag.
    /// </summary>
    /// <param name="raised">if it should be raised or not</param>
    public MultipleUseFlag(bool raised = false)
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
        if (!Raise())
        {
            throw new InvalidOperationException($"Repeated Raise for a {nameof(MultipleUseFlag)} instance");
        }
    }

    /// <summary>
    /// Lowers the flag. If already low, throws InvalidOperationException.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LowerOrDie()
    {
        if (!Lower())
        {
            throw new InvalidOperationException($"Repeated Lower for a {nameof(MultipleUseFlag)} instance");
        }
    }

    /// <summary>
    /// Lowers the flag
    /// </summary>
    /// <returns>If already low, false; otherwise, true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Lower()
    {
        return Interlocked.CompareExchange(ref _state, 0, 1) == 1;
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
    public static implicit operator bool(MultipleUseFlag flag)
    {
        return flag.IsRaised();
    }
}
