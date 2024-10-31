using Certificates.Validation;
using Domain.Options;
using Npgsql;

namespace Database.Connections;

// This class is used as a common base for all projects that need to connect to the database.
// This handles the mutual certificate authentication.
public class DatabaseConnectionFactory(DatabaseConnectionOptions options)
{
    public async Task<NpgsqlConnection> OpenConnection()
    {
        var connection = new NpgsqlConnection(options.ConnectionString);

        // This instructs Npgsql to present the client certificate to the server.
        connection.ProvideClientCertificatesCallback =
            certCollection => certCollection.Add(options.ClientCertificate);

        // This instructs Npgsql to validate the server certificate and the Common Name of the server.
        // Manually implementing the validation is necessary to allow for validation of private CAs.
        // "sender" has nothing to do with the server but contains information about the caller of the callback.
        // "sslPolicyErrors" contains the validation results using the system's default validation. For details visit
        // https://learn.microsoft.com/en-us/dotnet/api/system.net.security.remotecertificatevalidationcallback?view=net-8.0#remarks
        // Both can be ignored in this context.
        connection.UserCertificateValidationCallback =
            (sender, cert, chain, sslPolicyErrors) =>
                CertificateValidation.ValidateSignedByCaWithCommonName(options.TrustedRootCertificates, cert, chain,
                    [options.CommonName]);

        await connection.OpenAsync();

        return connection;
    }
}