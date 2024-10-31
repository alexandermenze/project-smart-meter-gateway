using System.Security.Cryptography.X509Certificates;
using Domain.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using UserWebservice.Configurations;

namespace UserWebservice.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection ConfigureKestrelServerLimitsAndCertificate(this IServiceCollection services,
        X509Certificate2 certificate)
    {
        services.Configure<KestrelServerOptions>(kestrelOpts =>
        {
            // Set 1 MB as the request body size limit to limit denial of service attacks.
            kestrelOpts.Limits.MaxRequestBodySize = 1 * 1024 * 1024;

            kestrelOpts.ConfigureHttpsDefaults(opts =>
            {
                // Serve the service certificate to callers of this service.
                // This enabled TLS encryption. Because the certificate is private, the client will have to trust it manually.
                opts.ServerCertificate = certificate;
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, JwtOptions jwtOptions)
    {
        // This instructs ASP.NET Core to use JWT tokens for authentication.
        // The token is read from the Authorization header and validated using the provided options.
        // The claims are automatically added to the HttpContext.User property.
        services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    // The same signing key has to be used for both issuing and validating the token.
                    IssuerSigningKey = jwtOptions.TokenSigningCredentials.Key,
                    ValidateLifetime = true,
                    // Validating the token would fail even without issuer and audience since this service is
                    // the only one issuing and processing tokens with the specified token key.
                    // It is good practice to enable them to future-proof the system in case more issuers or audiences are added.
                    ValidateIssuer = true,
                    ValidIssuer = TokenConstants.Issuer,
                    ValidateAudience = true,
                    ValidAudience = TokenConstants.Audience
                };
            });

        return services;
    }
}