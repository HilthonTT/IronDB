using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace IronDB.Transport.Tcp;

/// <summary>
/// SSL/TLS TCP connection. Implementation pending — the factory currently throws
/// <see cref="NotImplementedException"/>.
/// </summary>
public static class TcpConnectionSsl
{
    public static ITcpConnection CreateConnectingConnection(
        Guid connectionId,
        string targetHost,
        string[] otherNames,
        IPEndPoint remoteEndPoint,
        CertificateDelegates.ServerCertificateValidator sslServerCertValidator,
        Func<X509CertificateCollection> clientCertificatesSelector,
        TcpClientConnector connector,
        TimeSpan connectionTimeout,
        Action<ITcpConnection>? onConnectionEstablished,
        Action<ITcpConnection, SocketError>? onConnectionFailed,
        bool verbose)
    {
        throw new NotImplementedException("SSL TCP connection is not implemented yet.");
    }
}
