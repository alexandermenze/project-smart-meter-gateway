using System.Security.Cryptography.X509Certificates;

namespace Domain.Options;

public record ServiceAuthOptions(
    X509Certificate2 Certificate,
    // Multiple root certificates are supported to allow for transition phases.
    IReadOnlyCollection<X509Certificate2> TrustedRootCertificates);