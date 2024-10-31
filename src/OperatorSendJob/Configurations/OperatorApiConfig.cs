namespace OperatorSendJob.Configurations;

public class OperatorApiConfig
{
    public const string Key = "OperatorApi";
    
    public required string BaseUrl { get; init; }
    public required string CommonName { get; init; }
}