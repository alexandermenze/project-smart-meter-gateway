using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Domain.Configurations;
using Domain.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Parsers;

// This class is responsible for parsing configuration sections into strongly typed options.
// This reduces the overhead in the services setup and allows for a structured configuration.
public class ConfigurationParser(IConfiguration configuration)
{
    public ConfigurationParser Get<T>(string key, out T value)
    {
        // A missing configuration section is considered a fatal error. This is a design decision that helps the
        // application to fail fast.
        value = configuration.GetSection(key).Get<T>() ??
                throw new InvalidOperationException($"Configuration with key '{key}' is missing.");

        return this;
    }

    public ConfigurationParser Map(out ServiceAuthOptions serviceAuthOptions)
    {
        Get(ServiceAuthConfig.Key, out ServiceAuthConfig serviceAuthConfig);

        var trustedRootCertificates = serviceAuthConfig.TrustedRootCertificates
            .Select(certificateString => new X509Certificate2(Encoding.UTF8.GetBytes(certificateString)))
            .ToImmutableArray();

        var clientCertificate =
            X509Certificate2.CreateFromPem(serviceAuthConfig.Certificate, serviceAuthConfig.PrivateKey);

        serviceAuthOptions = new ServiceAuthOptions(clientCertificate, trustedRootCertificates);

        return this;
    }

    public ConfigurationParser Map(out DatabaseConnectionOptions databaseConnectionOptions)
    {
        Map(out ServiceAuthOptions serviceAuthOptions);
        Get(DatabaseConfiguration.Key, out DatabaseConfiguration databaseConfiguration);

        databaseConnectionOptions = new DatabaseConnectionOptions(
            databaseConfiguration.ConnectionString,
            databaseConfiguration.CommonName,
            serviceAuthOptions.TrustedRootCertificates,
            serviceAuthOptions.Certificate);

        return this;
    }

    public ConfigurationParser Map(out JwtOptions jwtOptions)
    {
        Get(UserAuthConfig.Key, out UserAuthConfig userAuthConfig);

        var jwtTokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userAuthConfig.TokenKey));

        jwtOptions = new JwtOptions(
            // The tokens are hashed and signed with the jwtTokenKey using the HMAC SHA-256 algorithm.
            // See https://www.rfc-editor.org/rfc/rfc8725
            new SigningCredentials(jwtTokenKey, SecurityAlgorithms.HmacSha256),
            TimeSpan.FromMinutes(userAuthConfig.TokenLifetimeMinutes));

        return this;
    }

    public static ConfigurationParser From(IConfiguration configuration) =>
        new(configuration);
}