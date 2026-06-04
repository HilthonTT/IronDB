using IronDB.Core.Threading;
using IronDB.Core.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IronDB.Core;

internal sealed class ObjectPool<T>
    where T : class
{
}

internal class ObjectPool<T, TResetBehavior>
    where T : class
    where TResetBehavior : struct, IResetSupport<T>
{
    private static readonly TResetBehavior _resetBehavior = new();

    private struct Element
    {
        internal T Value;
    }

    // <remarks>
    /// Not using System.Func{T} because this file is linked into the (debugger) Formatter,
    /// which does not have that type (since it compiles against .NET 2.0).
    /// </remarks>
    public delegate T Factory();

    // Storage for the pool objects. The first item is stored in a dedicated field because we
    // expect to be able to satisfy most requests from it.
    private T _firstItem = default!;
    private readonly Element[] _items = [];

    // factory is stored for the lifetime of the pool. We will call this only when pool needs to
    // expand. compared to "new T()", Func gives more flexibility to implementers and faster
    // than "new T()".
    private readonly Factory _factory = default!;

#if DETECT_LEAKS
    private static readonly ConditionalWeakTable<T, LeakTracker> LeakTrackers = [];

    private sealed class LeakTracker : IDisposable
    {
        private SingleUseFlag _disposed = new();

#if TRACE_LEAKS
        internal volatile object Trace = null;
#endif

        public void Dispose()
        {
            _disposed.Raise();
            GC.SuppressFinalize(this);
        }

        private static string GetTrace()
        {
#if TRACE_LEAKS
            return Trace == null ? "" : Trace.ToString();
#else
            return "Leak tracing information is disabled. Define TRACE_LEAKS on ObjectPool`1.cs to get more info \n";
#endif
        }
    }

    ~LeakTracker()
    {
        if (!_disposed.IsRaised() && !Environment.HasShutdownStarted)
        {
            var trace = GetTrace();

            // If you are seeing this message it means that object has been allocated from the pool 
            // and has not been returned back. This is not critical, but turns pool into rather 
            // inefficient kind of "new".
            Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nPool detected potential leaking of {typeof(T)}. \n Location of the leak: \n {GetTrace()} TRACEOBJECTPOOLLEAKS_END");
        }
    }
}
#endif

    public ObjectPool(Factory factory)
        : this(factory, ProcessorInfo.ProcessorCount * 2)
    {
    }

    public ObjectPool(Factory factory, int size)
    {
        Debug.Assert(size >= 1);
        _factory = factory;
        _items = new Element[size - 1];
    }

    private T CreateInstance()
    {
        var inst = _factory();
        return inst;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ObjectPoolContext<T, TResetBehavior> AllocateInContext()
    {
        return new ObjectPoolContext<T, TResetBehavior>(this, Allocate());
    }

    /// <summary>
    /// Produces an instance.
    /// </summary>
    /// <remarks>
    /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
    /// Note that Free will try to store recycled objects close to the start thus statistically 
    /// reducing how far we will typically search.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Allocate()
    {
        // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
        // Note that the initial read is optimistically not synchronized. That is intentional. 
        // We will interlock only when we have a candidate. in a worst case we may miss some
        // recently returned objects. Not a big deal.
        T inst = _firstItem;
        if (inst is null || inst != Interlocked.CompareExchange(ref _firstItem!, null, inst))
        {
            inst = AllocateSlow();
        }

#if DETECT_LEAKS
            var tracker = new LeakTracker();
            LeakTrackers.Add(inst, tracker);

#if TRACE_LEAKS
            var frame = CaptureStackTrace();
            tracker.Trace = frame;
#endif
#endif
        return inst;
    }


    private T AllocateSlow()
    {
        var items = _items;

        for (int i = 0; i < items.Length; i++)
        {
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            T inst = items[i].Value;
            if (inst is null)
            {
                continue;
            }

            if (inst == Interlocked.CompareExchange(ref items[i].Value!, null, inst))
            {
                return inst;
            }
        }

        return CreateInstance();
    }

    /// <summary>
    /// Returns objects to the pool.
    /// </summary>
    /// <remarks>
    /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
    /// Note that Free will try to store recycled objects close to the start thus statistically 
    /// reducing how far we will typically search in Allocate.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Free(T obj)
    {
        Validate(obj);
        ForgetTrackedObject(obj);

        _resetBehavior.Reset(obj);

        if (_firstItem == null)
        {
            // Intentionally not using interlocked here. 
            // In a worst case scenario two objects may be stored into same slot.
            // It is very unlikely to happen and will only mean that one of the objects will get collected.
            _firstItem = obj;
        }
        else
        {
            FreeSlow(obj);
        }
    }

    private void FreeSlow(T obj)
    {
        var items = _items;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Value is null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                items[i].Value = obj;
                break;
            }
        }
    }

    /// <summary>
    /// Removes an object from leak tracking.  
    /// 
    /// This is called when an object is returned to the pool.  It may also be explicitly 
    /// called if an object allocated from the pool is intentionally not being returned
    /// to the pool.  This can be of use with pooled arrays if the consumer wants to 
    /// return a larger array to the pool than was originally allocated.
    /// </summary>
    [Conditional("DEBUG")]
    [Conditional("DETECT_LEAKS")]
    public void ForgetTrackedObject(T old, T? replacement = null)
    {
#if DETECT_LEAKS
            if (LeakTrackers.TryGetValue(old, out LeakTracker tracker))
            {
                tracker.Dispose();
                LeakTrackers.Remove(old);
            }
            else
            {
                var trace = CaptureStackTrace();
                Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nObject of type {typeof(T)} was freed, but was not from pool. \n Callstack: \n {trace} TRACEOBJECTPOOLLEAKS_END");
            }

            if (replacement != null)
            {
                tracker = new LeakTracker();
                LeakTrackers.Add(replacement, tracker);
            }
#endif
    }

#if DETECT_LEAKS
        private static Lazy<Type> _stackTraceType = new(() => Type.GetType("System.Diagnostics.StackTrace")!);

        private static object? CaptureStackTrace()
        {
            return Activator.CreateInstance(_stackTraceType.Value);
        }
#endif

    [Conditional("DEBUG")]
    [Conditional("DETECT_LEAKS")]
    private void Validate(object obj)
    {
        Debug.Assert(obj != null, "freeing null?");

        var items = _items;
        for (int i = 0; i < items.Length; i++)
        {
            var value = items[i].Value;
            if (value == null)
            {
                return;
            }

            Debug.Assert(value != obj, "freeing twice?");
        }
    }
}
