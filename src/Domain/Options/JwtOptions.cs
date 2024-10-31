using Microsoft.IdentityModel.Tokens;

namespace Domain.Options;

public record JwtOptions(
    SigningCredentials TokenSigningCredentials,
    TimeSpan TokenLifetime);