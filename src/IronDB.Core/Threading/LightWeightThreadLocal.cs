using System.Collections.Concurrent;

namespace IronDB.Core.Threading;

public sealed class LightWeightThreadLocal<T> : IDisposable
{
    [ThreadStatic]
    private static CurrentThreadState? _state;

    private readonly WeakReferenceCompareValue<LightWeightThreadLocal<T>> SelfReference;
    private ConcurrentDictionary<WeakReferenceCompareValue<CurrentThreadState>, T?> _values = new();
    private readonly Func<T>? _generator;
    private bool _disposed;
    private static int GlobalVersion;

    public LightWeightThreadLocal(Func<T>? generator = null)
    {
        _generator = generator;
        SelfReference = new WeakReferenceCompareValue<LightWeightThreadLocal<T>>(this);
    }

    public ICollection<T?> Values => _disposed 
        ? throw new ObjectDisposedException(nameof(LightWeightThreadLocal<>))
        : _values.Values;

    public bool IsValueCreated => _disposed
                ? throw new ObjectDisposedException(nameof(LightWeightThreadLocal<>))
                : _state != null && _values.ContainsKey(_state.SelfReference);

    public T? Value
    {
        get
        {
            if (!_disposed)
            {
                (_state ??= new CurrentThreadState()).Register(this);
                if (!_values.TryGetValue(_state.SelfReference, out T? v) && _generator is not null)
                {
                    v = _generator();
                    _values[_state.SelfReference] = v;
                }
                return v;
            }

            throw new ObjectDisposedException(nameof(LightWeightThreadLocal<>));
        }

        set
        {
            if (!_disposed)
            {
                (_state ??= new CurrentThreadState()).Register(this);
                _values[_state.SelfReference] = value;
            }
            else
            {
                throw new ObjectDisposedException(nameof(LightWeightThreadLocal<>));
            }
        }
    }

    public void Dispose()
    {
        var copy = _values;
        if (copy is null)
        {
            return;
        }

        copy = Interlocked.CompareExchange(ref _values!, null, copy);
        if (copy is null)
        {
            return;
        }

        Interlocked.Increment(ref GlobalVersion);
        _disposed = true;
        _values = new();
    }

    private sealed class CurrentThreadState
    {
        private readonly HashSet<WeakReferenceCompareValue<LightWeightThreadLocal<T>>> _parents = new();

        public readonly WeakReferenceCompareValue<CurrentThreadState> SelfReference;

        private readonly LocalState _localState = new();

        public CurrentThreadState()
        {
            SelfReference = new WeakReferenceCompareValue<CurrentThreadState>(this);
        }

        public void Register(LightWeightThreadLocal<T> parent)
        {
            _parents.Add(parent.SelfReference);
            int localVersion = _localState.LocalVersion;
            var globalVersion = GlobalVersion;
            if (localVersion != globalVersion)
            {
                // a thread local instance was disposed, let's check
                // if we need to do cleanup here
                RemoveDisposedParents();
                _localState.LocalVersion = globalVersion;
            }
        }

        private void RemoveDisposedParents()
        {
            var toRemove = new List<WeakReferenceCompareValue<LightWeightThreadLocal<T>>>();
            foreach (var local in _parents)
            {
                if (!local.TryGetTarget(out LightWeightThreadLocal<T>? target) || target?._disposed == true)
                {
                    toRemove.Add(local);
                }
            }

            foreach (var remove in toRemove)
            {
                _parents.Remove(remove);
            }
        }

        ~CurrentThreadState()
        {
            foreach (var parent in _parents)
            {
                if (parent.TryGetTarget(out var liveParent) == false)
                {
                    continue;
                }

                var copy = liveParent?._values;
                if (copy is null)
                {
                    continue;
                }
                copy.TryRemove(SelfReference, out _);
            }
        }
    }

    private sealed class WeakReferenceCompareValue<TK> : IEquatable<WeakReferenceCompareValue<TK>>
        where TK : class
    {
        private readonly WeakReference<TK> _weak;
        private readonly int _hashCode;

        public bool TryGetTarget(out TK? target)
        {
            return _weak.TryGetTarget(out target);
        }

        public WeakReferenceCompareValue(TK instance)
        {
            _hashCode = instance.GetHashCode();
            _weak = new WeakReference<TK>(instance);
        }

        public bool Equals(WeakReferenceCompareValue<TK>? other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (_hashCode != other._hashCode)
            {
                return false;
            }

            if (_weak.TryGetTarget(out var x) == false ||
                other._weak.TryGetTarget(out var y) == false)
            {
                return false;
            }
            return ReferenceEquals(x, y);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() == typeof(TK))
            {
                int hashCode = obj.GetHashCode();
                if (hashCode != _hashCode)
                {
                    return false;
                }
                if (_weak.TryGetTarget(out var other) == false)
                {
                    return false;
                }
                return ReferenceEquals(other, obj);
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((WeakReferenceCompareValue<TK>)obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }

    private sealed class LocalState
    {
        public int LocalVersion;
    }
}