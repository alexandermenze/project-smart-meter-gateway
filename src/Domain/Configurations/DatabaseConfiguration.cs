namespace Domain.Configurations;

// Used in services: All
// Configuration section for database connection options
public class DatabaseConfiguration
{
    public const string Key = "Database";
    
    // Connection string holding the hostname, port, username and additional connection options for the database
    public required string ConnectionString { get; init; }

    // Expected common name for the database service
    public required string CommonName { get; init; }
}
