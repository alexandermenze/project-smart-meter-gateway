using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Certificates.Validation;
using Domain.Options;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace SmartMeterWebservice.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection ConfigureCertificateAuthentication(this IServiceCollection services,
        ServiceAuthOptions serviceAuthOptions, IReadOnlyCollection<string> allowedCommonNames)
    {
        // Certificate authentication is configured on two levels:
        // 1. Webserver (Kestrel): This is the pre-authentication level that negotiates the TLS connection.
        services.ConfigureKestrelCertificateAuthentication(serviceAuthOptions, allowedCommonNames);
        // 2. Application (ASP.NET Core): This is the authentication level that extracts information from the certificate.
        // Only configuring application level authentication is not enough as the connection is already established at that moment.
        services.ConfigureApplicationCertificateAuthentication(serviceAuthOptions);

        return services;
    }

    private static void ConfigureKestrelCertificateAuthentication(this IServiceCollection services,
        ServiceAuthOptions serviceAuthOptions, IReadOnlyCollection<string> allowedCommonNames)
    {
        // Set up the webserver Kestrel that hosts the ASP.NET Core application to require client certificates.
        // This has to be configured since TLS is negotiated before the request hits our application code.
        // Connections without certificates or certificates from other, including public, CAs are disconnected immediately.
        services.Configure<KestrelServerOptions>(kestrelOpts =>
        {
            // Set 1 MB as the request body size limit to limit denial of service attacks.
            kestrelOpts.Limits.MaxRequestBodySize = 1 * 1024 * 1024;

            kestrelOpts.ConfigureHttpsDefaults(opts =>
            {
                // Serve the service certificate to callers of this service.
                opts.ServerCertificate = serviceAuthOptions.Certificate;

                // Reject all requests without client certificates.
                opts.ClientCertificateMode = ClientCertificateMode.RequireCertificate;

                // Validate the caller certificate against our private CA.
                opts.ClientCertificateValidation = (cert, chain, sslPolicyErrors) =>
                    CertificateValidation.ValidateSignedByCaWithCommonName(serviceAuthOptions.TrustedRootCertificates,
                        cert, chain, allowedCommonNames);
            });
        });
    }

    private static void ConfigureApplicationCertificateAuthentication(this IServiceCollection services,
        ServiceAuthOptions serviceAuthOptions)
    {
        // This is the authentication of ASP.NET Core. While both levels of authentication check whether the certificate
        // is valid and from our private CA, ASP.NET Core is more flexible and allows for the extraction of information from
        // the certificate in the form of claims.
        services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate(opts =>
            {
                // The revocation check has to be disabled for our private CA as that is not implemented for this project.
                opts.RevocationMode = X509RevocationMode.NoCheck;

                // Accept only certificates signed by our private CA. Publicly trusted certificates are invalid in this context.
                // Since ASP.NET Core does not support a custom certificate validation callback,
                // the common CertificateValidation.ValidateSignedByCa method cannot be used here.
                opts.AllowedCertificateTypes = CertificateTypes.Chained;
                opts.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
                opts.CustomTrustStore =
                    new X509Certificate2Collection(serviceAuthOptions.TrustedRootCertificates.ToArray());

                // Make sure the certificate is issued for client authentication. Validates the Extended Key Usage field.
                opts.ValidateCertificateUse = true;

                opts.Events = new CertificateAuthenticationEvents
                {
                    // Callback after successful certificate validation.
                    // This is the place to extract information from the certificate.
                    OnCertificateValidated = MapClientCertificateCommonNameToClaims
                };
            });
    }

    private static Task MapClientCertificateCommonNameToClaims(CertificateValidatedContext context)
    {
        // Extract the smart meter id from the certificate subject name and store it in the claims.
        // The simple name is the common name specified in the certificate.

        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier,
                    context.ClientCertificate.GetNameInfo(X509NameType.SimpleName, false))
            ],
            context.Scheme.Name));

        context.Success();

        return Task.CompletedTask;
    }
}