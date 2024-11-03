namespace Domain.Configurations;

// Used in services: UserWebservice
// Configurations for the tokens issued by the webservice
public class UserAuthConfig
{
    public const string Key = "UserAuth";

    // A key used as the token signing key (should have a length of 128 bits)
    public required string TokenKey { get; init; }
    
    // Token lifetime in minutes should be a short duration to avoid token theft. After that the token will expire.
    public required int TokenLifetimeMinutes { get; init; }
}
