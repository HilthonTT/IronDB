using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using IronDB.Common.Utils;
using Serilog;

namespace IronDB.Transport.Tcp;

public class TcpClientConnector : IDisposable
{
    private const int CheckPeriodMs = 200;

    private static readonly ILogger Logger = Log.ForContext<TcpClientConnector>();

    private readonly SocketArgsPool _connectSocketArgsPool;
    private readonly ConcurrentDictionary<Guid, PendingConnection> _pendingConnections;
    private readonly Timer _timer;
    private bool _disposed;

    public TcpClientConnector()
    {
        _connectSocketArgsPool = new SocketArgsPool(
            "TcpClientConnector._connectSocketArgsPool",
            TcpConfiguration.ConnectPoolSize,
            CreateConnectSocketArgs);
        _pendingConnections = new ConcurrentDictionary<Guid, PendingConnection>();
        _timer = new Timer(TimerCallback);
        // prevent possible null reference exceptions in case of slow initialization
        _timer.Change(CheckPeriodMs, Timeout.Infinite);
    }

    private SocketAsyncEventArgs CreateConnectSocketArgs()
    {
        var socketArgs = new SocketAsyncEventArgs();
        socketArgs.Completed += ConnectCompleted;
        socketArgs.UserToken = new CallbacksStateToken();
        return socketArgs;
    }

    public ITcpConnection ConnectTo(
        Guid connectionId,
        IPEndPoint remoteEndPoint,
        TimeSpan connectionTimeout,
        Action<ITcpConnection>? onConnectionEstablished = null,
        Action<ITcpConnection, SocketError>? onConnectionFailed = null,
        bool verbose = true)
    {
        Ensure.NotNull(remoteEndPoint);
        return TcpConnection.CreateConnectingTcpConnection(
            connectionId, remoteEndPoint, this, connectionTimeout,
            onConnectionEstablished, onConnectionFailed, verbose);
    }

    public ITcpConnection ConnectSslTo(
        Guid connectionId,
        string targetHost,
        string[] otherNames,
        IPEndPoint remoteEndPoint,
        TimeSpan connectionTimeout,
        CertificateDelegates.ServerCertificateValidator sslServerCertValidator,
        Func<X509CertificateCollection> clientCertificatesSelector,
        Action<ITcpConnection>? onConnectionEstablished = null,
        Action<ITcpConnection, SocketError>? onConnectionFailed = null,
        bool verbose = true)
    {
        Ensure.NotNull(remoteEndPoint);
        return TcpConnectionSsl.CreateConnectingConnection(
            connectionId, targetHost, otherNames, remoteEndPoint, sslServerCertValidator,
            clientCertificatesSelector, this, connectionTimeout,
            onConnectionEstablished, onConnectionFailed, verbose);
    }

    internal void InitConnect(
        IPEndPoint serverEndPoint,
        Action<Socket> onSocketAssigned,
        Action<IPEndPoint, Socket> onConnectionEstablished,
        Action<IPEndPoint, SocketError> onConnectionFailed,
        ITcpConnection connection,
        TimeSpan connectionTimeout)
    {
        ArgumentNullException.ThrowIfNull(serverEndPoint);
        ArgumentNullException.ThrowIfNull(onSocketAssigned);
        ArgumentNullException.ThrowIfNull(onConnectionEstablished);
        ArgumentNullException.ThrowIfNull(onConnectionFailed);

        var socketArgs = _connectSocketArgsPool.Get();
        var connectingSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        onSocketAssigned(connectingSocket);
        socketArgs.RemoteEndPoint = serverEndPoint;
        socketArgs.AcceptSocket = connectingSocket;
        var callbacks = (CallbacksStateToken)socketArgs.UserToken!;
        callbacks.OnConnectionEstablished = onConnectionEstablished;
        callbacks.OnConnectionFailed = onConnectionFailed;
        callbacks.PendingConnection = new PendingConnection(connection, DateTime.UtcNow.Add(connectionTimeout));

        AddToConnecting(callbacks.PendingConnection);

        try
        {
            bool firedAsync = connectingSocket.ConnectAsync(socketArgs);
            if (!firedAsync)
            {
                ProcessConnect(socketArgs);
            }
        }
        catch (ObjectDisposedException)
        {
            HandleBadConnect(socketArgs);
        }
    }

    private void ConnectCompleted(object? sender, SocketAsyncEventArgs e) => ProcessConnect(e);

    private void ProcessConnect(SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            HandleBadConnect(e);
        }
        else
        {
            OnSocketConnected(e);
        }
    }

    private void HandleBadConnect(SocketAsyncEventArgs socketArgs)
    {
        var serverEndPoint = (IPEndPoint?)socketArgs.RemoteEndPoint;
        var socketError = socketArgs.SocketError;
        var callbacks = (CallbacksStateToken)socketArgs.UserToken!;
        var onConnectionFailed = callbacks.OnConnectionFailed;
        var pendingConnection = callbacks.PendingConnection;

        Helper.EatException(() => socketArgs.AcceptSocket?.Close());
        socketArgs.AcceptSocket = null;
        callbacks.Reset();
        _connectSocketArgsPool.Return(socketArgs);

        if (pendingConnection is not null && RemoveFromConnecting(pendingConnection)
            && serverEndPoint is not null && onConnectionFailed is not null)
        {
            onConnectionFailed(serverEndPoint, socketError);
        }
    }

    private void OnSocketConnected(SocketAsyncEventArgs socketArgs)
    {
        var remoteEndPoint = (IPEndPoint?)socketArgs.RemoteEndPoint;
        var socket = socketArgs.AcceptSocket;
        var callbacks = (CallbacksStateToken)socketArgs.UserToken!;
        var onConnectionEstablished = callbacks.OnConnectionEstablished;
        var pendingConnection = callbacks.PendingConnection;

        socketArgs.AcceptSocket = null;
        callbacks.Reset();
        _connectSocketArgsPool.Return(socketArgs);

        if (pendingConnection is not null && RemoveFromConnecting(pendingConnection)
            && remoteEndPoint is not null && socket is not null && onConnectionEstablished is not null)
        {
            onConnectionEstablished(remoteEndPoint, socket);
        }
    }

    private void TimerCallback(object? state)
    {
        foreach (var pendingConnection in _pendingConnections.Values)
        {
            if (DateTime.UtcNow >= pendingConnection.WhenToKill && RemoveFromConnecting(pendingConnection))
            {
                Helper.EatException(() => pendingConnection.Connection.Close("Connection establishment timeout."));
            }
        }

        try
        {
            _timer.Change(CheckPeriodMs, Timeout.Infinite);
        }
        catch (ObjectDisposedException)
        {
            // ignore
        }
    }

    private void AddToConnecting(PendingConnection pendingConnection)
        => _pendingConnections.TryAdd(pendingConnection.Connection.ConnectionId, pendingConnection);

    private bool RemoveFromConnecting(PendingConnection pendingConnection)
    {
        if (pendingConnection.Connection is null)
        {
            Logger.Warning("Network Card disconnected");
            return false;
        }

        return _pendingConnections.TryRemove(pendingConnection.Connection.ConnectionId, out PendingConnection? conn)
               && conn is not null
               && Interlocked.CompareExchange(ref conn.Done, 1, 0) == 0;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _timer.Dispose();
        }
        _disposed = true;
    }

    private sealed class CallbacksStateToken
    {
        public Action<IPEndPoint, Socket>? OnConnectionEstablished;
        public Action<IPEndPoint, SocketError>? OnConnectionFailed;
        public PendingConnection? PendingConnection;

        public void Reset()
        {
            OnConnectionEstablished = null;
            OnConnectionFailed = null;
            PendingConnection = null;
        }
    }

    private sealed class PendingConnection
    {
        public ITcpConnection Connection { get; }
        public DateTime WhenToKill { get; }
        public int Done;

        public PendingConnection(ITcpConnection connection, DateTime whenToKill)
        {
            Connection = connection;
            WhenToKill = whenToKill;
        }
    }
}
