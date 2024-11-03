namespace OperatorSendJob.Configurations;

// Used in services: OperatorSendJob
// This defines the configuration for access to the operator's endpoint
public class OperatorApiConfig
{
    public const string Key = "OperatorApi";
    
    // URL at which to find the operator api
    public required string BaseUrl { get; init; }
    // Expected certificate common name for the operator endpoint
    public required string CommonName { get; init; }
}
