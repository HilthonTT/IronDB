using System.Net;
using System.Net.Sockets;
using IronDB.BufferManagement;
using IronDB.Common.Utils;
using Serilog;

namespace IronDB.Transport.Tcp;

public class TcpConnection : TcpConnectionBase, ITcpConnection, IDisposable
{
    internal const int MaxSendPacketSize = 65535 /*Max IP packet size*/ - 20 /*IP packet header size*/ - 32 /*TCP min header size*/;

    internal static readonly BufferManager BufferManager =
        new(TcpConfiguration.BufferChunksCount, TcpConfiguration.SocketBufferSize);

    private static readonly ILogger Logger = Log.ForContext<TcpConnection>();

    private static readonly SocketArgsPool SocketArgsPool = new(
        "TcpConnection.SocketArgsPool",
        TcpConfiguration.SendReceivePoolSize,
        () => new SocketAsyncEventArgs());

    private readonly Guid _connectionId;
    private readonly bool _verbose;
    private string _clientConnectionName = string.Empty;

    private Socket? _socket;
    private SocketAsyncEventArgs? _receiveSocketArgs;
    private SocketAsyncEventArgs? _sendSocketArgs;

    private readonly ConcurrentQueueWrapper<ArraySegment<byte>> _sendQueue = new();
    private readonly Queue<ReceivedData> _receiveQueue = new();
    private readonly MemoryStream _memoryStream = new();
    private long _memoryStreamOffset;

    private readonly Lock _receivingLock = new();
    private readonly Lock _sendLock = new();
    private readonly Lock _closeLock = new();
    private bool _isSending;
    private volatile bool _isClosed;
    private volatile bool _isClosing;
    private bool _disposed;

    private Action<ITcpConnection, IEnumerable<ArraySegment<byte>>>? _receiveCallback;

    public event Action<ITcpConnection, SocketError>? ConnectionClosed;

    public Guid ConnectionId => _connectionId;

    public int SendQueueSize => _sendQueue.Count;

    public string ClientConnectionName => _clientConnectionName;

    public IPEndPoint RemoteEndPoint => RemoteEndPointField;

    public IPEndPoint LocalEndPoint => LocalEndPointInternal ?? RemoteEndPointField;

    public static ITcpConnection CreateConnectingTcpConnection(
        Guid connectionId,
        IPEndPoint remoteEndPoint,
        TcpClientConnector connector,
        TimeSpan connectionTimeout,
        Action<ITcpConnection>? onConnectionEstablished,
        Action<ITcpConnection, SocketError>? onConnectionFailed,
        bool verbose)
    {
        ArgumentNullException.ThrowIfNull(connector);
        var connection = new TcpConnection(connectionId, remoteEndPoint, verbose);
        connector.InitConnect(
            remoteEndPoint,
            socket => connection.InitSocket(socket),
            (_, _) =>
            {
                connection.InitSendReceive();
                onConnectionEstablished?.Invoke(connection);
            },
            (_, socketError) => onConnectionFailed?.Invoke(connection, socketError),
            connection,
            connectionTimeout);
        return connection;
    }

    public static ITcpConnection CreateAcceptedTcpConnection(
        Guid connectionId, IPEndPoint remoteEndPoint, Socket socket, bool verbose)
    {
        var connection = new TcpConnection(connectionId, remoteEndPoint, verbose);
        connection.InitSocket(socket);
        connection.InitSendReceive();
        return connection;
    }

    private TcpConnection(Guid connectionId, IPEndPoint remoteEndPoint, bool verbose)
        : base(remoteEndPoint)
    {
        Ensure.NotEmptyGuid(connectionId);

        _connectionId = connectionId;
        _verbose = verbose;
    }

    private void InitSocket(Socket socket) => _socket = socket;

    private void InitSendReceive()
    {
        if (_socket is null)
        {
            CloseInternal(SocketError.Shutdown, "Socket not initialised.");
            return;
        }

        InitConnectionBase(_socket);
        lock (_sendLock)
        {
            try
            {
                _socket.NoDelay = true;
            }
            catch (ObjectDisposedException)
            {
                CloseInternal(SocketError.Shutdown, "Socket disposed.");
                return;
            }
            catch (SocketException)
            {
                CloseInternal(SocketError.Shutdown, "Socket is disposed.");
                return;
            }

            var receiveSocketArgs = SocketArgsPool.Get();
            _receiveSocketArgs = receiveSocketArgs;
            _receiveSocketArgs.AcceptSocket = _socket;
            _receiveSocketArgs.Completed += OnReceiveAsyncCompleted;

            var sendSocketArgs = SocketArgsPool.Get();
            _sendSocketArgs = sendSocketArgs;
            _sendSocketArgs.AcceptSocket = _socket;
            _sendSocketArgs.Completed += OnSendAsyncCompleted;
        }

        StartReceive();
        TrySend();
    }

    public void EnqueueSend(IEnumerable<ArraySegment<byte>> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        lock (_sendLock)
        {
            int bytes = 0;
            foreach (var segment in data)
            {
                _sendQueue.Enqueue(segment);
                bytes += segment.Count;
            }

            NotifySendScheduled(bytes);
        }

        TrySend();
    }

    private void TrySend()
    {
        bool continueSendSynchronously = true;
        try
        {
            do
            {
                lock (_sendLock)
                {
                    if (_isSending || (_sendQueue.IsEmpty && _memoryStreamOffset >= _memoryStream.Length) || _sendSocketArgs is null)
                    {
                        return;
                    }
                    if (TcpConnectionMonitor.Default.IsSendBlocked())
                    {
                        return;
                    }
                    _isSending = true;
                }

                if (_memoryStreamOffset >= _memoryStream.Length)
                {
                    _memoryStream.SetLength(0);
                    _memoryStreamOffset = 0L;

                    while (_sendQueue.TryDequeue(out ArraySegment<byte> sendPiece))
                    {
                        if (sendPiece.Array is not null)
                        {
                            _memoryStream.Write(sendPiece.Array, sendPiece.Offset, sendPiece.Count);
                        }
                        if (_memoryStream.Length >= MaxSendPacketSize)
                        {
                            break;
                        }
                    }
                }

                int sendingBytes = Math.Min((int)_memoryStream.Length - (int)_memoryStreamOffset, MaxSendPacketSize);

                var sendArgs = _sendSocketArgs;
                if (sendArgs is null)
                {
                    return;
                }

                sendArgs.SetBuffer(_memoryStream.GetBuffer(), (int)_memoryStreamOffset, sendingBytes);
                _memoryStreamOffset += sendingBytes;

                NotifySendStarting(sendArgs.Count);
                bool firedAsync = sendArgs.AcceptSocket!.SendAsync(sendArgs);
                if (firedAsync)
                {
                    continueSendSynchronously = false;
                }
                else
                {
                    continueSendSynchronously = ProcessSend(sendArgs);
                }
            }
            while (continueSendSynchronously);
        }
        catch (ObjectDisposedException)
        {
            ReturnSendingSocketArgs();
        }
    }

    private void OnSendAsyncCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (ProcessSend(e))
        {
            TrySend();
        }
    }

    private bool ProcessSend(SocketAsyncEventArgs socketArgs)
    {
        if (socketArgs.SocketError != SocketError.Success)
        {
            NotifySendCompleted(0);
            ReturnSendingSocketArgs();
            CloseInternal(socketArgs.SocketError, "Socket send error.");
            return false;
        }

        NotifySendCompleted(socketArgs.Count);

        if (_isClosed)
        {
            ReturnSendingSocketArgs();
            return false;
        }

        lock (_sendLock)
        {
            _isSending = false;
        }
        return true;
    }

    public void ReceiveAsync(Action<ITcpConnection, IEnumerable<ArraySegment<byte>>> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        lock (_receivingLock)
        {
            if (_receiveCallback is not null)
            {
                Logger.Fatal("ReceiveAsync called again while previous call was not fulfilled");
                throw new InvalidOperationException(
                    "ReceiveAsync called again while previous call was not fulfilled.");
            }

            _receiveCallback = callback;
        }

        TryDequeueReceivedData();
    }

    private void StartReceive()
    {
        try
        {
            bool continueReceiveSynchronously = true;

            do
            {
                var receiveArgs = _receiveSocketArgs;
                if (receiveArgs is null)
                {
                    return;
                }

                var buffer = BufferManager.CheckOut();
                if (buffer.Array is null || buffer.Count == 0 || buffer.Array.Length < buffer.Offset + buffer.Count)
                {
                    throw new InvalidBufferException("Invalid buffer allocated.");
                }

                // TODO: do we need to lock on _receiveSocketArgs?
                lock (receiveArgs)
                {
                    receiveArgs.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
                    if (receiveArgs.Buffer is null)
                    {
                        throw new InvalidOperationException("Buffer was not set.");
                    }
                }

                NotifyReceiveStarting();
                bool firedAsync;
                lock (receiveArgs)
                {
                    if (receiveArgs.Buffer is null)
                    {
                        throw new InvalidOperationException("Buffer was lost.");
                    }
                    firedAsync = receiveArgs.AcceptSocket!.ReceiveAsync(receiveArgs);
                }

                if (firedAsync)
                {
                    continueReceiveSynchronously = false;
                }
                else
                {
                    bool processReceiveSuccess = ProcessReceive(receiveArgs);
                    if (processReceiveSuccess)
                    {
                        TryDequeueReceivedData();
                    }

                    continueReceiveSynchronously = processReceiveSuccess;
                }
            }
            while (continueReceiveSynchronously);
        }
        catch (ObjectDisposedException)
        {
            ReturnReceivingSocketArgs();
        }
    }

    private void OnReceiveAsyncCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (ProcessReceive(e))
        {
            TryDequeueReceivedData();
            StartReceive();
        }
    }

    private bool ProcessReceive(SocketAsyncEventArgs socketArgs)
    {
        // socket closed normally or some error occurred
        if (socketArgs.BytesTransferred == 0 || socketArgs.SocketError != SocketError.Success)
        {
            NotifyReceiveCompleted(0);
            ReturnReceivingSocketArgs();
            CloseInternal(
                socketArgs.SocketError,
                socketArgs.SocketError != SocketError.Success ? "Socket receive error" : "Socket closed");
            return false;
        }

        NotifyReceiveCompleted(socketArgs.BytesTransferred);

        if (socketArgs.Buffer is null)
        {
            throw new InvalidOperationException("Receive completed but buffer is null.");
        }

        lock (_receivingLock)
        {
            var buf = new ArraySegment<byte>(socketArgs.Buffer, socketArgs.Offset, socketArgs.Count);
            _receiveQueue.Enqueue(new ReceivedData(buf, socketArgs.BytesTransferred));
        }

        var receiveArgs = _receiveSocketArgs;
        if (receiveArgs is not null)
        {
            lock (receiveArgs)
            {
                if (socketArgs.Buffer is null)
                {
                    throw new InvalidOperationException("Cleaning already null buffer.");
                }
                socketArgs.SetBuffer(null, 0, 0);
            }
        }

        return true;
    }

    private void TryDequeueReceivedData()
    {
        Action<ITcpConnection, IEnumerable<ArraySegment<byte>>>? callback;
        List<ReceivedData> res;
        lock (_receivingLock)
        {
            // no awaiting callback or no data to dequeue
            if (_receiveCallback is null || _receiveQueue.Count == 0)
            {
                return;
            }

            res = new List<ReceivedData>(_receiveQueue.Count);
            while (_receiveQueue.Count > 0)
            {
                res.Add(_receiveQueue.Dequeue());
            }

            callback = _receiveCallback;
            _receiveCallback = null;
        }

        var data = new ArraySegment<byte>[res.Count];
        int bytes = 0;
        for (int i = 0; i < data.Length; ++i)
        {
            var d = res[i];
            bytes += d.DataLen;
            data[i] = new ArraySegment<byte>(d.Buf.Array ?? [], d.Buf.Offset, d.DataLen);
        }

        lock (_closeLock)
        {
            if (!_isClosed)
            {
                callback(this, data);
            }
        }

        for (int i = 0, n = res.Count; i < n; ++i)
        {
            BufferManager.CheckIn(res[i].Buf); // dispose buffers
        }

        NotifyReceiveDispatched(bytes);
    }

    public void Close(string reason)
        => CloseInternal(SocketError.Success, reason ?? "Normal socket close.");

    private void CloseInternal(SocketError socketError, string reason)
    {
        lock (_closeLock)
        {
            if (_isClosing)
            {
                return;
            }
            _isClosing = true;
        }

        if (_socket is not null)
        {
            Helper.EatException(() => _socket.Shutdown(SocketShutdown.Both));
            Helper.EatException(() => _socket.Close());
        }

        lock (_closeLock)
        {
            _isClosed = true;
        }

        NotifyClosed();

        if (_verbose)
        {
            Logger.Information(
                "DB {connectionType} closed [{dateTime:HH:mm:ss.fff}: N{remoteEndPoint}, L{localEndPoint}, {connectionId:B}]:Received bytes: {totalBytesReceived}, Sent bytes: {totalBytesSent}",
                GetType().Name, DateTime.UtcNow, RemoteEndPoint, LocalEndPoint, _connectionId,
                TotalBytesReceived, TotalBytesSent);
            Logger.Information(
                "DB {connectionType} closed [{dateTime:HH:mm:ss.fff}: N{remoteEndPoint}, L{localEndPoint}, {connectionId:B}]:Send calls: {sendCalls}, callbacks: {sendCallbacks}",
                GetType().Name, DateTime.UtcNow, RemoteEndPoint, LocalEndPoint, _connectionId,
                SendCalls, SendCallbacks);
            Logger.Information(
                "DB {connectionType} closed [{dateTime:HH:mm:ss.fff}: N{remoteEndPoint}, L{localEndPoint}, {connectionId:B}]:Receive calls: {receiveCalls}, callbacks: {receiveCallbacks}",
                GetType().Name, DateTime.UtcNow, RemoteEndPoint, LocalEndPoint, _connectionId,
                ReceiveCalls, ReceiveCallbacks);
            Logger.Information(
                "DB {connectionType} closed [{dateTime:HH:mm:ss.fff}: N{remoteEndPoint}, L{localEndPoint}, {connectionId:B}]:Close reason: [{socketError}] {reason}",
                GetType().Name, DateTime.UtcNow, RemoteEndPoint, LocalEndPoint, _connectionId,
                socketError, reason);
        }

        lock (_sendLock)
        {
            if (!_isSending)
            {
                ReturnSendingSocketArgs();
            }
        }

        ConnectionClosed?.Invoke(this, socketError);
    }

    private void ReturnSendingSocketArgs()
    {
        var socketArgs = Interlocked.Exchange(ref _sendSocketArgs, null);
        if (socketArgs is null)
        {
            return;
        }
        socketArgs.Completed -= OnSendAsyncCompleted;
        socketArgs.AcceptSocket = null;
        if (socketArgs.Buffer is not null)
        {
            socketArgs.SetBuffer(null, 0, 0);
        }
        SocketArgsPool.Return(socketArgs);
    }

    private void ReturnReceivingSocketArgs()
    {
        var socketArgs = Interlocked.Exchange(ref _receiveSocketArgs, null);
        if (socketArgs is null)
        {
            return;
        }
        socketArgs.Completed -= OnReceiveAsyncCompleted;
        socketArgs.AcceptSocket = null;
        if (socketArgs.Buffer is not null)
        {
            BufferManager.CheckIn(
                new ArraySegment<byte>(socketArgs.Buffer, socketArgs.Offset, socketArgs.Count));
            socketArgs.SetBuffer(null, 0, 0);
        }
        SocketArgsPool.Return(socketArgs);
    }

    public void SetClientConnectionName(string clientConnectionName)
    {
        ArgumentNullException.ThrowIfNull(clientConnectionName);
        _clientConnectionName = clientConnectionName;
    }

    public override string ToString() => RemoteEndPoint.ToString();

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
            _memoryStream.Dispose();
        }
        _disposed = true;
    }

    private readonly struct ReceivedData(ArraySegment<byte> buf, int dataLen)
    {
        public ArraySegment<byte> Buf { get; } = buf;
        public int DataLen { get; } = dataLen;
    }
}
