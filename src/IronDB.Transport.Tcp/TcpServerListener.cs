using IronDB.Common.Utils;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace IronDB.Transport.Tcp;

public sealed class TcpServerListener : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<TcpServerListener>();

    private readonly IPEndPoint _serverEndPoint;
    private readonly Socket _listeningSocket;
    private readonly SocketArgsPool _acceptSocketArgsPool;
    private Action<IPEndPoint?, Socket>? _onSocketAccepted;

    public TcpServerListener(IPEndPoint serverEndPoint)
    {
        Ensure.NotNull(serverEndPoint, nameof(serverEndPoint));

        _serverEndPoint = serverEndPoint;

        _listeningSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        _acceptSocketArgsPool = new SocketArgsPool("TcpServerListener.AcceptSocketArgsPool",
            TcpConfiguration.ConcurrentAccepts * 2,
            CreateAcceptSocketArgs);
    }

    public void StartListening(Action<IPEndPoint?, Socket> callback, string securityType)
    {
        Ensure.NotNull(callback, nameof(callback));

        _onSocketAccepted = callback;

        Logger.Information(
            "Starting {securityType} TCP listening on TCP endpoint: {serverEndPoint}.",
            securityType,
            _serverEndPoint);

        try
        {
            _listeningSocket.ExclusiveAddressUse = true;
            _listeningSocket.Bind(_serverEndPoint);
            _listeningSocket.Listen(TcpConfiguration.AcceptBacklogCount);
        }
        catch (Exception)
        {
            Log.Information("Failed to listen on TCP endpoint: {serverEndPoint}.", _serverEndPoint);
            Helper.EatException(() => _listeningSocket.Close(TcpConfiguration.SocketCloseTimeoutSecs));
            throw;
        }

        for (int i = 0; i < TcpConfiguration.ConcurrentAccepts; ++i)
        {
            StartAccepting();
        }
    }

    private void StartAccepting()
    {
        SocketAsyncEventArgs socketArgs = _acceptSocketArgsPool.Get();

        try
        {
            var firedAsync = _listeningSocket.AcceptAsync(socketArgs);
            if (!firedAsync)
            {
                ProcessAccept(socketArgs);
            }
        }
        catch (ObjectDisposedException)
        {
            HandleBadAccept(socketArgs);
        }
    }

    private void AcceptCompleted(object? sender, SocketAsyncEventArgs e)
    {
        ProcessAccept(e);
    }

    private void ProcessAccept(SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            HandleBadAccept(e);
        }
        else
        {
            Socket? acceptSocket = e.AcceptSocket;
            e.AcceptSocket = null;
            _acceptSocketArgsPool.Return(e);

            if (acceptSocket is not null)
            {
                OnSocketAccepted(acceptSocket);
            }
        }

        StartAccepting();
    }

    public void Stop()
    {
        Helper.EatException(() => _listeningSocket.Close(TcpConfiguration.SocketCloseTimeoutSecs));
    }

    private void HandleBadAccept(SocketAsyncEventArgs socketArgs)
    {
        Helper.EatException(
            () => {
                socketArgs.AcceptSocket?.Close(TcpConfiguration.SocketCloseTimeoutSecs);
            });
        socketArgs.AcceptSocket = null;
        _acceptSocketArgsPool.Return(socketArgs);
    }

    private void OnSocketAccepted(Socket socket)
    {
        IPEndPoint? socketEndPoint;
        try
        {
            socketEndPoint = (IPEndPoint?)socket.RemoteEndPoint;
        }
        catch (Exception)
        {
            return;
        }

        _onSocketAccepted?.Invoke(socketEndPoint, socket);
    }

    private SocketAsyncEventArgs CreateAcceptSocketArgs()
    {
        var socketArgs = new SocketAsyncEventArgs();
        socketArgs.Completed += AcceptCompleted;
        return socketArgs;
    }

    public void Dispose()
    {
        _listeningSocket.Dispose();
    }
}
