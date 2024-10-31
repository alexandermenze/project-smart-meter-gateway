using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SmartMeterWebservice.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection ConfigureCertificateAuthorization(this IServiceCollection services,
        IReadOnlyCollection<string> allowedCommonNames)
    {
        // Configure ASP.NET Core to handle authorization using policies.
        // This ensures that only configured smart meters are allowed to save data.
        // This prevents certificates from obsolete devices to be used to send data.
        // Every endpoint is configured to require authentication and authorization
        // if not explicitly configured otherwise: Secure Defaults.

        var isConfiguredMeterPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(ClaimTypes.NameIdentifier, allowedCommonNames)
            .Build();

        services.AddAuthorizationBuilder()
            // Fallback is used in case no authorization is explicitly configured for an endpoint.
            .SetFallbackPolicy(isConfiguredMeterPolicy)
            // Default is used when authorization is configured for an endpoint, without specifying a policy.
            .SetDefaultPolicy(isConfiguredMeterPolicy);

        return services;
    }
}