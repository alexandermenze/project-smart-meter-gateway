using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KestrelServerOptions>(opts =>
{
    opts.ConfigureHttpsDefaults(httpsOpts =>
    {
        httpsOpts.ServerCertificate = X509Certificate2.CreateFromPem(
            "-----BEGIN CERTIFICATE-----\n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\n-----END CERTIFICATE-----",
            "-----BEGIN PRIVATE KEY-----\n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\nhQrFIYq49HvCsrnipb84YOPZ+fiYvw==\n-----END PRIVATE KEY-----");
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/api/consumptionValues", () => Results.Ok("Got it!"));

await app.RunAsync();