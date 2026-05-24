using System.Collections.Concurrent;
using System.Net.Sockets;

namespace IronDB.Transport.Tcp;

public sealed class SocketArgsPool
{
    private readonly Func<SocketAsyncEventArgs> _socketArgsCreator;
    private readonly ConcurrentStack<SocketAsyncEventArgs> _socketArgsPool = new();

    public string Name { get; }

    public SocketArgsPool(string name, int initialCount, Func<SocketAsyncEventArgs> socketArgsCreator)
    {
        ArgumentNullException.ThrowIfNull(socketArgsCreator);
        ArgumentOutOfRangeException.ThrowIfNegative(initialCount);

        Name = name;
        _socketArgsCreator = socketArgsCreator;

        for (int i = 0; i < initialCount; ++i)
        {
            _socketArgsPool.Push(socketArgsCreator());
        }
    }

    public SocketAsyncEventArgs Get()
        => _socketArgsPool.TryPop(out SocketAsyncEventArgs? result) ? result : _socketArgsCreator();

    public void Return(SocketAsyncEventArgs socketArgs)
    {
        ArgumentNullException.ThrowIfNull(socketArgs);
        _socketArgsPool.Push(socketArgs);
    }
}
