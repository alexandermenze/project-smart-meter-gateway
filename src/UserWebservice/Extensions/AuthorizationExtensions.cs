using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace UserWebservice.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection ConfigureJwtAuthorizationRequiresNameIdentifier(this IServiceCollection services)
    {
        // The authorization policy requires two things:
        // 1. The user must be authenticated.
        // 2. The user must have a claim with the type NameIdentifier. This is used in the endpoints.
        var requireAuthenticatedUserWithSubject = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(ClaimTypes.NameIdentifier)
            .Build();

        // The above policy is used as the default and fallback policy, applicable to all endpoints.
        services.AddAuthorizationBuilder()
            // Default is used when authorization is explicitly configured for an endpoint but no other policy.
            .SetDefaultPolicy(requireAuthenticatedUserWithSubject)
            // Fallback is used in case no authorization is configured for an endpoint explicitly.
            .SetFallbackPolicy(requireAuthenticatedUserWithSubject);

        return services;
    }
}