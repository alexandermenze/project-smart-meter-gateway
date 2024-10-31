using Database.Connections;
using Database.Repositories;
using Domain.Options;
using Domain.Parsers;
using SmartMeterWebservice;
using SmartMeterWebservice.Configurations;
using SmartMeterWebservice.Extensions;

var builder = WebApplication.CreateBuilder(args);

ConfigurationParser.From(builder.Configuration)
    .Get(SmartMeterAuthConfig.Key, out SmartMeterAuthConfig smartMeterAuthConfig)
    .Map(out ServiceAuthOptions serviceAuthOptions)
    .Map(out DatabaseConnectionOptions databaseConnectionOptions);

builder.Services.AddSingleton(databaseConnectionOptions);

// This configures the webserver (Kestrel) and the application to perform certificate authentication.
builder.Services.ConfigureCertificateAuthentication(serviceAuthOptions, smartMeterAuthConfig.AllowedCommonNames);

// This configures the application to perform authorization based on the common names of the client certificates.
// Only smart meters with configured Common Names are allowed to save data.
builder.Services.ConfigureCertificateAuthorization(smartMeterAuthConfig.AllowedCommonNames);

// Register the database dependencies and repositories
builder.Services.AddTransient<DatabaseConnectionFactory>();
builder.Services.AddTransient<ConsumptionValueRepository>();

// Allow Dapper to map the column names with underscores to the C# properties
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

// Make sure to redirect all requests to HTTPS as the first step in the pipeline.
// This is a precaution as HTTP should not even be configured in the first place.
app.UseHttpsRedirection();

// Enable the authentication and authorization pipelines for ALL incoming requests as configured above.
app.UseAuthentication();
// This is the authorization middleware that checks the common name of the client certificate and rejects any unknown smart meters.
app.UseAuthorization();

// Enable the consumption value endpoint
app.MapConsumptionValueEndpoint();

await app.RunAsync();