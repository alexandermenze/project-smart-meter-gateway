namespace Domain.Configurations;

// This class is used in all services as a common structure for the service authentication configuration.
public class ServiceAuthConfig
{
    public const string Key = "ServiceAuth";

    public required string Certificate { get; init; }
    public required string PrivateKey { get; init; }
    public required IReadOnlyList<string> TrustedRootCertificates { get; init; }
}