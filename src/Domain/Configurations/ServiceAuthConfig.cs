namespace Domain.Configurations;

// Used in services: All
// This class is used in all services as a common structure for the service authentication configuration. Certificate and PrivateKey refer to the credentials to the service uses for authentication against other services.
public class ServiceAuthConfig
{
    public const string Key = "ServiceAuth";

    public required string Certificate { get; init; }
    public required string PrivateKey { get; init; }

    // These are the root certificates the communication partners certificate is verified against
    public required IReadOnlyList<string> TrustedRootCertificates { get; init; }
}
