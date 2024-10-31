using System.Security.Cryptography.X509Certificates;

namespace Certificates.Validation;

public static class CertificateValidation
{
    // This logic is used to validate the certificate chain as well as the Common Name of the certificate.
    // Checking the Common Name is necessary to ensure, that the communication partner is the expected one.
    // This is used for connections to the Operator server and the database.
    public static bool ValidateSignedByCaWithCommonName(
        IReadOnlyCollection<X509Certificate2> validCAs,
        X509Certificate? certificateForValidation,
        X509Chain? chain,
        IReadOnlyCollection<string> allowedCommonNames)
    {
        if (certificateForValidation is null || chain is null)
            return false;

        if (ValidateSignedByCa(validCAs, certificateForValidation, chain) is false)
            return false;

        // Check the Common Name of the certificate against the allowed values. Using ordinal comparison to avoid
        // culture-specific comparisons. This avoids for example that 'a' is treated the same as 'ä', or 'ss' as 'ß'.
        // This is important to prevent spoofing attacks.
        return allowedCommonNames.Any(commonName => string.Equals(
            new X509Certificate2(certificateForValidation).GetNameInfo(X509NameType.SimpleName, false),
            commonName,
            StringComparison.Ordinal));
    }
    
    // This logic is used in multiple services to validate the certificate chain for private CAs.
    // The method returns true if the certificate is signed by one of the provided CAs.
    // The system root certificates are not considered to reduce the risk of a compromised root CA or a
    // misconfigured system.
    private static bool ValidateSignedByCa(
        IReadOnlyCollection<X509Certificate2> validCAs,
        X509Certificate certificateForValidation,
        X509Chain? chain)
    {
        // Reject, if the certificate is not part of a chain.
        if (chain is null)
            return false;

        var certificate2 = new X509Certificate2(certificateForValidation);

        // Allow only custom CA certificates as configured.
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.AddRange(validCAs.ToArray());

        // Returns true if the chain is valid and signed by one of the provided CAs.
        return chain.Build(certificate2);
    }
}