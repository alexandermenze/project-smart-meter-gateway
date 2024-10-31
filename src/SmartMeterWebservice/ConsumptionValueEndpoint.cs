using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Database.Repositories;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using SmartMeterWebservice.Dtos;

namespace SmartMeterWebservice;

public static class ConsumptionValueEndpoint
{
    public static void MapConsumptionValueEndpoint(this WebApplication app)
    {
        app.MapPost("/api/consumptionValues", async (ConsumptionValueRequest request, HttpContext context,
            [FromServices] ConsumptionValueRepository repository) =>
        {
            // ASP.NET Core handles the input parsing and validation.
            // If the request does not match the ConsumptionValueRequest model this method is not executed.

            // Try to get the name identifier which was set during authentication.
            var meterId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // If no claim is set for the current user, the user is not authorized properly.
            // This should never happen, but has to be handled.
            if (meterId is null)
                return Results.Forbid();

            // Reject requests that are not signed using the smart meter's private key.
            // This ensures that only cryptographically secured data can enter the system.
            if (VerifySignature(request, context.Connection.ClientCertificate) is false)
                return Results.BadRequest("Signature verification failed!");

            // The meterId is set to the extracted Common Name that is saved in the name identifier claim.
            var consumptionValue = new ConsumptionValue(0, meterId, DateTime.UtcNow, request.PeriodValue,
                request.PeriodStart, request.PeriodEnd, request.Signature, false);

            // Save the consumption value to the database.
            await repository.Insert(consumptionValue);

            return Results.Created();
        });
    }

    private static bool VerifySignature(ConsumptionValueRequest request, X509Certificate2? certificate)
    {
        var rsaPublicKey = certificate?.PublicKey.GetRSAPublicKey();

        // If the certificate is not present or the public key cannot be extracted, the request is rejected.
        // Fail Securely
        if (rsaPublicKey is null)
            return false;

        var data = string.Create(CultureInfo.InvariantCulture,
            $"{request.PeriodValue}_{request.PeriodStart:O}_{request.PeriodEnd:O}");

        // The signature is base64 encoded and has to be decoded before it can be verified.
        var binarySignature = Convert.FromBase64String(request.Signature);

        // The requests authorized certificate is used to validate the signature.
        // By checking the signatures additional security is added but flexibility is reduced. Changing the 
        // signature algorithm requires a change in the smart meter, the webservice and on the operator side.
        return rsaPublicKey.VerifyData(Encoding.UTF8.GetBytes(data), binarySignature, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }
}