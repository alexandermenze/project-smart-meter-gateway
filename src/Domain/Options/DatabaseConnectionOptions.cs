using System.Security.Cryptography.X509Certificates;

namespace Domain.Options;

public record DatabaseConnectionOptions(
    string ConnectionString,
    string CommonName,
    // The root certificates used to verify the server's certificate. During transitions, the old and new certificates may be used.
    IReadOnlyCollection<X509Certificate2> TrustedRootCertificates,
    X509Certificate2 ClientCertificate);