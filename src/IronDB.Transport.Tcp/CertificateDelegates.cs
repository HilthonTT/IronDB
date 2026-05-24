using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace IronDB.Transport.Tcp;

/// <summary>
/// Holders for TLS certificate-validation and -selection callbacks used by the SSL TCP connection paths.
/// </summary>
public static class CertificateDelegates
{
    /// <summary>
    /// Validates the remote (server) certificate during the TLS handshake.
    /// Returns true if the certificate should be accepted.
    /// </summary>
    public delegate (bool Accepted, string? Error) ServerCertificateValidator(
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors,
        string[]? otherNames);

    /// <summary>
    /// Selects the client certificate to present during mutual TLS.
    /// </summary>
    public delegate X509Certificate? ClientCertificateSelector(
        object sender,
        string targetHost,
        X509CertificateCollection localCertificates,
        X509Certificate? remoteCertificate,
        string[] acceptableIssuers);
}
