using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Threading;

public sealed class SharedMultipleUseFlag
{
    private MultipleUseFlag _flag;

    /// <summary>
    /// This flag is always lowered and should not be raised by anyone.
    /// </summary>
    public static SharedMultipleUseFlag None = new(false);

    /// <summary>
    /// Copy constructor. DO NOT USE.
    /// </summary>
    public SharedMultipleUseFlag(SharedMultipleUseFlag copy)
    {
        throw new NotImplementedException($"Copy of {nameof(SharedMultipleUseFlag)} is disallowed");
    }

    /// <summary>
    /// Creates a lowered flag.
    /// </summary>
    public SharedMultipleUseFlag() : this(false)
    {
    }

    /// <summary>
    /// Creates a flag.
    /// </summary>
    /// <param name="raised">if it should be raised or not</param>
    public SharedMultipleUseFlag(bool raised = false)
    {
        _flag = new MultipleUseFlag(raised);
    }

    /// <summary>
    /// Raises the flag. If already up, throws InvalidOperationException.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaiseOrDie()
    {
        _flag.RaiseOrDie();
    }

    /// <summary>
    /// Lowers the flag. If already low, throws InvalidOperationException.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LowerOrDie()
    {
        _flag.LowerOrDie();
    }

    /// <summary>
    /// Lowers the flag
    /// </summary>
    /// <returns>If already low, false; otherwise, true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Lower()
    {
        return _flag.Lower();
    }

    /// <summary>
    /// Raises the flag
    /// </summary>
    /// <returns>If already raised, false; otherwise, true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Raise()
    {
        return _flag.Raise();
    }

    /// <returns>True iff the flag is raised</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRaised()
    {
        return _flag.IsRaised();
    }

    /// <summary>
    /// Returns true iff the flag is raised. Same as calling IsRaised().
    /// </summary>
    /// <param name="flag">Flag to check</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(SharedMultipleUseFlag flag)
    {
        Debug.Assert(flag is not null);
        return flag.IsRaised();
    }
}