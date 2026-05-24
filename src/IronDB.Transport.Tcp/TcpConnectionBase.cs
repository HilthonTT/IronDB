using System.Net;
using System.Net.Sockets;
using IronDB.Common.Utils;

namespace IronDB.Transport.Tcp;

public class TcpConnectionBase : IMonitoredTcpConnection
{
    private Socket? _socket;
    private IPEndPoint? _localEndPoint;

    private long _lastSendStarted = -1;
    private long _lastReceiveStarted = -1;
    private bool _isClosed;

    private int _pendingSendBytes;
    private int _inSendBytes;
    private int _pendingReceivedBytes;
    private long _totalBytesSent;
    private long _totalBytesReceived;

    private int _sentAsyncs;
    private int _sentAsyncCallbacks;
    private int _recvAsyncs;
    private int _recvAsyncCallbacks;

    protected IPEndPoint RemoteEndPointField { get; }

    protected TcpConnectionBase(IPEndPoint remoteEndPoint)
    {
        Ensure.NotNull(remoteEndPoint);
        RemoteEndPointField = remoteEndPoint;

        TcpConnectionMonitor.Default.Register(this);
    }

    protected void InitConnectionBase(Socket socket)
    {
        Ensure.NotNull(socket);

        _socket = socket;
        _localEndPoint = Helper.EatException(() => (IPEndPoint?)socket.LocalEndPoint);
    }

    protected IPEndPoint? LocalEndPointInternal => _localEndPoint;

    protected void NotifySendScheduled(int bytes)
        => Interlocked.Add(ref _pendingSendBytes, bytes);

    protected void NotifySendStarting(int bytes)
    {
        if (Interlocked.CompareExchange(ref _lastSendStarted, DateTime.UtcNow.Ticks, -1) != -1)
        {
            throw new InvalidOperationException("Concurrent send detected.");
        }
        Interlocked.Add(ref _pendingSendBytes, -bytes);
        Interlocked.Add(ref _inSendBytes, bytes);
        Interlocked.Increment(ref _sentAsyncs);
    }

    protected void NotifySendCompleted(int bytes)
    {
        Interlocked.Exchange(ref _lastSendStarted, -1);
        Interlocked.Add(ref _inSendBytes, -bytes);
        Interlocked.Add(ref _totalBytesSent, bytes);
        Interlocked.Increment(ref _sentAsyncCallbacks);
    }

    protected void NotifyReceiveStarting()
    {
        if (Interlocked.CompareExchange(ref _lastReceiveStarted, DateTime.UtcNow.Ticks, -1) != -1)
        {
            throw new InvalidOperationException("Concurrent receive detected.");
        }

        Interlocked.Increment(ref _recvAsyncs);
    }

    protected void NotifyReceiveCompleted(int bytes)
    {
        Interlocked.Exchange(ref _lastReceiveStarted, -1);
        Interlocked.Add(ref _pendingReceivedBytes, bytes);
        Interlocked.Add(ref _totalBytesReceived, bytes);
        Interlocked.Increment(ref _recvAsyncCallbacks);
    }

    protected void NotifyReceiveDispatched(int bytes)
        => Interlocked.Add(ref _pendingReceivedBytes, -bytes);

    protected void NotifyClosed()
    {
        _isClosed = true;
        TcpConnectionMonitor.Default.Unregister(this);
    }

    public bool IsReadyForSend => Interlocked.Read(ref _lastSendStarted) >= 0;

    public bool IsReadyForReceive
    {
        get
        {
            try
            {
                return _socket is not null && !_isClosed && _socket.Poll(0, SelectMode.SelectRead);
            }
            catch (ObjectDisposedException)
            {
                //TODO: why do we get this?
                return false;
            }
        }
    }

    public bool IsInitialized => _socket is not null;

    public bool IsFaulted
    {
        get
        {
            try
            {
                return _socket is not null && !_isClosed && _socket.Poll(0, SelectMode.SelectError);
            }
            catch (ObjectDisposedException)
            {
                //TODO: why do we get this?
                return false;
            }
        }
    }

    public bool IsClosed => _isClosed;

    public bool InSend => Interlocked.Read(ref _lastSendStarted) >= 0;

    public bool InReceive => Interlocked.Read(ref _lastReceiveStarted) >= 0;

    public DateTime? LastSendStarted
    {
        get
        {
            long ticks = Interlocked.Read(ref _lastSendStarted);
            return ticks >= 0 ? new DateTime(ticks) : null;
        }
    }

    public DateTime? LastReceiveStarted
    {
        get
        {
            long ticks = Interlocked.Read(ref _lastReceiveStarted);
            return ticks >= 0 ? new DateTime(ticks) : null;
        }
    }

    public int PendingSendBytes => _pendingSendBytes;

    public int InSendBytes => _inSendBytes;

    public int PendingReceivedBytes => _pendingReceivedBytes;

    public long TotalBytesSent => _totalBytesSent;

    public long TotalBytesReceived => _totalBytesReceived;

    public int SendCalls => _sentAsyncs;

    public int SendCallbacks => _sentAsyncCallbacks;

    public int ReceiveCalls => _recvAsyncs;

    public int ReceiveCallbacks => _recvAsyncCallbacks;
}
