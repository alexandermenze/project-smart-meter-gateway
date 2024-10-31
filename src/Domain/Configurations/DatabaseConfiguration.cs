namespace Domain.Configurations;

public class DatabaseConfiguration
{
    public const string Key = "Database";
    
    public required string ConnectionString { get; init; }
    public required string CommonName { get; init; }
}