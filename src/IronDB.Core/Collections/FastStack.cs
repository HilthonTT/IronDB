using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Collections;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
// A simple stack of objects.  Internally it is implemented as an array,
// so Push can be O(n).  Pop is O(1).
[DebuggerDisplay("Count = {Count}")]
public sealed class FastStack<T> : IEnumerable<T>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    private const int DefaultCapacity = 4;

    private T[] _array = [];     // Storage for stack elements
    private int _size;           // Number of items in the stack.
    private int _version;        // Used to keep enumerator in sync w/ collection.

    // Create a stack with a specific initial capacity.  The initial capacity
    // must be a non-negative number.
    public FastStack(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));

        _array = new T[capacity];
    }

    public int Count => _size;

    /// <summary>
    /// Removes all objects from the Stack but clearing the backing array unless the type is native.
    /// </summary>
    public void Clear()
    {
        if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(byte) ||
            typeof(T) == typeof(short) || typeof(T) == typeof(long) || typeof(T) == typeof(ulong) ||
            typeof(T) == typeof(nint) || typeof(T) == typeof(nuint) || typeof(T) == typeof(IntPtr))
        {
            _size = 0;
            _version++;

            return;
        }

        int size = _size;

        _size = 0;
        _version++;
        if (size > 0)
        {
            Array.Clear(_array, 0, size); // Clear the elements so that the gc can reclaim the references.
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Returns the top object on the stack without removing it.  If the stack
    // is empty, Peek throws an InvalidOperationException.        
    public T Peek()
    {
        if (_size == 0)
        {
            goto Error;
        }

        return _array[_size - 1];

    Error:
        return ThrowForEmptyStack();
    }

    // Returns the top object on the stack without removing it.  If the stack
    // is empty, Peek throws an InvalidOperationException.        
    public ref T TopByRef()
    {
        if (_size != 0)
            return ref _array[_size - 1];

        throw new InvalidOperationException("The stack is empty.");
    }

    public bool TryPeek(out T result)
    {
        if (_size == 0)
        {
            result = default!;
            return false;
        }

        result = _array[_size - 1];
        return true;
    }

    public bool TryPeek(int depth, out T result)
    {
        if (_size < depth || depth <= 0)
        {
            result = default!;
            return false;
        }

        result = _array[_size - depth];
        return true;
    }

    // Pops an item from the top of the stack.  If the stack is empty, Pop
    // throws an InvalidOperationException.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        if (_size == 0)
        {
            throw new InvalidOperationException("The stack is empty.");
        }

        _version++;
        T item = _array[--_size];
        _array[_size] = default!;

        return item;
    }

    public bool TryPop(out T result)
    {
        if (_size == 0)
        {
            result = default!;
            return false;
        }

        _version++;
        result = _array[--_size];
        _array[_size] = default!;

        return true;
    }

    // Pushes an item to the top of the stack.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(in T item)
    {
        if (_size == _array.Length)
            goto Grow;

        _array[_size++] = item;
        _version++;
        return;

    Grow:
        PushUnlikely(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T PushByRef()
    {
        if (_size != _array.Length)
        {
            ref var item = ref _array[_size++];
            _version++;
            return ref item!;
        }

        return ref PushUnlikelyByRef();
    }

    private ref T PushUnlikelyByRef()
    {
        Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
        ref var item = ref _array[_size++];
        _version++;

        return ref item!;
    }

    private void PushUnlikely(T item)
    {
        Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
        _array[_size++] = item;
        _version++;
    }

    // Copies the Stack to an array, in the same order Pop would return the items.
    public T[] ToArray()
    {
        if (_size == 0)
            return Array.Empty<T>();

        T[] objArray = new T[_size];
        int i = 0;
        while (i < _size)
        {
            objArray[i] = _array[_size - i - 1];
            i++;
        }
        return objArray;
    }

    private T ThrowForEmptyStack()
    {
        Debug.Assert(_size == 0);
        throw new InvalidOperationException("The stack is empty.");
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly FastStack<T> _stack;
        private readonly int _version;
        private int _index;
        private T _currentElement;

        internal Enumerator(FastStack<T> stack)
        {
            _stack = stack;
            _version = stack._version;
            _index = -2;
            _currentElement = default!;
        }

        public void Dispose()
        {
            _index = -1;
        }

        public bool MoveNext()
        {
            bool retval;
            if (_version != _stack._version)
            {
                throw new InvalidOperationException("version mismatch");
            }

            if (_index == -2)
            {  // First call to enumerator.
                _index = _stack._size - 1;
                retval = (_index >= 0);
                if (retval)
                {
                    _currentElement = _stack._array[_index];
                }
                return retval;
            }
            if (_index == -1)
            {  // End of enumeration.
                return false;
            }

            retval = (--_index >= 0);
            _currentElement = retval ? _stack._array[_index] : default!;

            return retval;
        }

        public readonly T Current
        {
            get
            {
                if (_index < 0)
                {
                    ThrowEnumerationNotStartedOrEnded();
                }
                return _currentElement;
            }
        }

        private readonly void ThrowEnumerationNotStartedOrEnded()
        {
            Debug.Assert(_index == -1 || _index == -2);
            throw new InvalidOperationException(_index == -2 ? "Enumeration has not started" : "Enumeration has already ended");
        }

        object IEnumerator.Current
        {
            get { return Current!; }
        }

        void IEnumerator.Reset()
        {
            if (_version != _stack._version)
            {
                throw new InvalidOperationException("version mismatch");
            }

            _index = -2;
            _currentElement = default!;
        }
    }
}
