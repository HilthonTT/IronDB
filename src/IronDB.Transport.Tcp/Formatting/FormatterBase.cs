using IronDB.BufferManagement;

namespace IronDB.Transport.Tcp.Formatting;

public abstract class FormatterBase<T> : IMessageFormatter<T>
{
    /// <summary>
	/// Gets a <see cref="BufferPool"></see> representing the IMessage provided.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A <see cref="BufferPool"></see> with a representation of the message</returns>
	public abstract BufferPool ToBufferPool(T message);

    public virtual ArraySegment<byte> ToArraySegment(T message)
    {
        return new ArraySegment<byte>(ToArray(message));
    }

    public virtual T From(BufferPool bufferPool)
    {
        ArgumentNullException.ThrowIfNull(bufferPool, nameof(bufferPool));
        var stream = new BufferPoolStream(bufferPool);
        return From(stream);
    }

    public virtual T From(ArraySegment<byte> segment)
    {
        using var stream = new MemoryStream(segment.Array ?? [], segment.Offset, segment.Count, false);
        return From(stream);
    }

    public virtual T From(byte[] array)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        using var stream = new MemoryStream(array, 0, array.Length, false);
        return From(stream);
    }

    public virtual byte[] ToArray(T message)
    {
        using BufferPool pool = ToBufferPool(message);
        return pool.ToByteArray();
    }

    /// <summary>
	/// Creates a message object from the specified stream
	/// </summary>
	/// <param name="stream">The stream.</param>
	/// <returns></returns>
	public abstract T From(Stream stream);
}
