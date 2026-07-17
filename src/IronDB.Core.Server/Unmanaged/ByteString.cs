using IronDB.Core.Json;
using IronDB.Core.Server.Unmanaged;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace IronDB.Core.Server.Unmanaged;

public unsafe struct ByteString : IEquatable<ByteString>
{
    public ByteStringStorage* _pointer;

#if VALIDATE
        internal ByteString(ByteStringStorage* ptr)
        {
            this._pointer = ptr;
            this.Key = ptr->Key; // We store the storage key
        }

        internal readonly ulong Key;
#else
    internal ByteString(ByteStringStorage* ptr)
    {
        _pointer = ptr;
#if DEBUG
        Generation = -1;
#endif
    }
#endif

#if DEBUG
    public int Generation;
#endif
    public ByteStringType Flags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(HasValue, "ByteString.HasValue");
            EnsureIsNotBadPointer();

            return _pointer->Flags;
        }
    }

    public readonly byte* Ptr
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(HasValue, "ByteString.HasValue");
            EnsureIsNotBadPointer();

            return _pointer->Ptr;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetUserDefinedFlags(ByteStringType flags)
    {
        if ((flags & ByteStringType.ByteStringMask) == 0)
        {
            _pointer->Flags |= flags;
            return;
        }

        ThrowFlagsWithReservedBits();
    }

    [DoesNotReturn]
    private static void ThrowFlagsWithReservedBits()
    {
        throw new ArgumentException("The flags passed contains reserved bits.");
    }

    public bool IsMutable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(HasValue, "ByteString.HasValue");
            EnsureIsNotBadPointer();

            return (_pointer->Flags & ByteStringType.Mutable) != 0;
        }
    }

    public bool IsExternal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(HasValue, "ByteString.HasValue");
            EnsureIsNotBadPointer();

            return (_pointer->Flags & ByteStringType.External) != 0;
        }
    }

    public readonly int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_pointer is null)
                return 0;

            EnsureIsNotBadPointer();

            return _pointer->Length;
        }
    }

    public int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_pointer is null)
                return 0;

            EnsureIsNotBadPointer();

            return _pointer->Size;
        }
    }

    public readonly bool HasValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _pointer != null && _pointer->Flags != ByteStringType.Disposed;
        }
    }

    public byte this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(HasValue, "ByteString.HasValue");
            EnsureIsNotBadPointer();

            return *(_pointer->Ptr + (sizeof(byte) * index));
        }
    }

    public void CopyTo(int from, byte* dest, int offset, int count)
    {
        Debug.Assert(HasValue, "ByteString.HasValue");

        if (from + count > _pointer->Length)
            throw new ArgumentOutOfRangeException(nameof(from), "Cannot copy data after the end of the slice");

        EnsureIsNotBadPointer();
        Memory.Copy(dest + offset, _pointer->Ptr + from, count);
    }

    public void CopyTo(Span<byte> dest)
    {
        ToSpan().CopyTo(dest);
    }

    public void CopyTo(byte* dest)
    {
        Debug.Assert(HasValue, "ByteString.HasValue");

        EnsureIsNotBadPointer();
        Memory.Copy(dest, _pointer->Ptr, _pointer->Length);
    }

    public void CopyTo(byte[] dest)
    {
        Debug.Assert(HasValue, "ByteString.HasValue");

        EnsureIsNotBadPointer();
        new Span<byte>(_pointer->Ptr, _pointer->Length).CopyTo(dest);
    }

#if VALIDATE

        [Conditional("VALIDATE")]
        internal readonly void EnsureIsNotBadPointer()
        {
            if (_pointer->Ptr == null)
                throw new InvalidOperationException("The inner storage pointer is not initialized. This is a defect on the implementation of the ByteStringContext class");

            if (_pointer->Key == ByteStringStorage.NullKey)
                throw new InvalidOperationException("The memory referenced has already being released. This is a dangling pointer. Check your .Release() statements and aliases in the calling code.");

            if ( this.Key != _pointer->Key)
            {
                if (this.Key >> 16 != _pointer->Key >> 16)
                    throw new InvalidOperationException("The owner context for the ByteString and the unmanaged storage are different. Make sure you havent killed the allocator and kept a reference to the ByteString outside of its scope.");

                Debug.Assert((this.Key & 0x0000000FFFFFFFF) != (_pointer->Key & 0x0000000FFFFFFFF), "(this.Key & 0x0000000FFFFFFFF) != (_pointer->Key & 0x0000000FFFFFFFF)");
                throw new InvalidOperationException("The key for the ByteString and the unmanaged storage are different. This is a dangling pointer. Check your .Release() statements and aliases in the calling code.");                                    
            }
        }

#else
    [Conditional("VALIDATE")]
    internal static void EnsureIsNotBadPointer() { }
#endif

    public readonly void Clear()
    {
        Memory.Set(Ptr, 0, Length);
    }

    public void CopyTo(int from, byte[] dest, int offset, int count)
    {
        Debug.Assert(HasValue, "ByteString.HasValue");

        if (from + count > _pointer->Length)
        {
            throw new ArgumentOutOfRangeException(nameof(from), "Cannot copy data after the end of the slice");
        }
        if (offset + count > dest.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(from), "Cannot copy data after the end of the buffer");
        }

        EnsureIsNotBadPointer();

        new Span<byte>(_pointer->Ptr + from, count)
            .CopyTo(dest.AsSpan(offset, count));
    }

    public override string ToString()
    {
        if (!HasValue)
        {
            return string.Empty;
        }

        EnsureIsNotBadPointer();

        return Encoding.UTF8.GetString(_pointer->Ptr, _pointer->Length);
    }

    public string ToString(UTF8Encoding encoding)
    {
        if (!HasValue)
        {
            return string.Empty;
        }

        EnsureIsNotBadPointer();

        return encoding.GetString(_pointer->Ptr, _pointer->Length);
    }

    public int IndexOf(byte c)
    {
        for (int i = 0; i < Length; i++)
        {
            if (this[i] == c)
            {
                return i;
            }
        }
        return -1;
    }

    public string Substring(int length)
    {
        if (!HasValue)
        {
            return string.Empty;
        }

        EnsureIsNotBadPointer();

        var encoding = Encodings.Utf8;
        return encoding.GetString(_pointer->Ptr, length);
    }

    public void Truncate(int newSize)
    {
        EnsureIsNotBadPointer();

        if (_pointer->Size < newSize || newSize < 0)
        {
            ThrowInvalidSize();
        }

        _pointer->Length = newSize;
    }

    [DoesNotReturn]
    private static void ThrowInvalidSize()
    {
        throw new ArgumentOutOfRangeException("newSize", "must be within the existing string limits");
    }


    public string ToString(Encoding encoding)
    {
        if (!HasValue)
        {
            return string.Empty;
        }

        EnsureIsNotBadPointer();

        return encoding.GetString(_pointer->Ptr, _pointer->Length);
    }

    [Obsolete("This is a reference comparison. Use SliceComparer or ByteString.Match instead.", error: true)]
#pragma warning disable CS0809
    public override readonly bool Equals(object? obj)
#pragma warning restore CS0809
    {
        return obj is ByteString @string && this == @string;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetContentHash()
    {
        // Given how the size of slices can vary it is better to lose a bit (10%) on smaller slices 
        // (less than 20 bytes) and to win big on the bigger ones. 
        //
        // After 24 bytes the gain is 10%
        // After 64 bytes the gain is 2x
        // After 128 bytes the gain is 4x.
        //
        // We should control the distribution of this over time.

        if (_pointer is null)
        {
            return 0;
        }

        return _pointer->GetContentHash();
    }

    public override int GetHashCode()
    {
        return (int)GetContentHash();
    }

    [Obsolete("This is a reference comparison. Use SliceComparer or ByteString.Match instead.", error: true)]
    public static bool operator ==(ByteString x, ByteString y)
    {
        return x._pointer == y._pointer;
    }

    [Obsolete("This is a reference comparison. Use SliceComparer or ByteString.Match instead.", error: true)]
    public static bool operator !=(ByteString x, ByteString y)
    {
        return !(x == y);
    }

    [Obsolete("This is a reference comparison. Use SliceComparer or ByteString.Match instead.", error: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(ByteString other)
    {
        return this == other;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Match(ByteString? other)
    {
        if (!other.HasValue)
        {
            return false;
        }

        ByteString otherValue = other.Value;

        return Length == otherValue.Length &&
               Memory.Compare(Ptr, otherValue.Ptr, Length) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Match(LazyStringValue other)
    {
        return Length == other.Length &&
               Memory.Compare(Ptr, other.Buffer, Length) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<byte> ToReadOnlySpan()
    {
        return new ReadOnlySpan<byte>(Ptr, Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<byte> ToSpan()
    {
        return new Span<byte>(Ptr, Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> ToSpan<T>() where T : unmanaged
    {
        return new Span<T>(Ptr, Length / Unsafe.SizeOf<T>());
    }
}
