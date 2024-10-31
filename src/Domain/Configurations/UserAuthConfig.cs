namespace Domain.Configurations;

public class UserAuthConfig
{
    public const string Key = "UserAuth";

    public required string TokenKey { get; init; }
    
    // Token lifetime in minutes should be a short duration to avoid token theft
    public required int TokenLifetimeMinutes { get; init; }
}