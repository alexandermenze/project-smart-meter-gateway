using Database.Connections;
using Database.Repositories;
using Domain.Options;
using Domain.Parsers;
using Microsoft.AspNetCore.Identity;
using UserWebservice.Extensions;

var builder = WebApplication.CreateBuilder(args);

ConfigurationParser.From(builder.Configuration)
    .Map(out JwtOptions jwtOptions)
    .Map(out ServiceAuthOptions serviceAuthOptions)
    .Map(out DatabaseConnectionOptions databaseConnectionOptions);

// Validate the token lifetime to prevent configuration errors
// The token lifetime should be balanced between security and usability:
// too short and users will be logged out too often, too long and the token can be abused for replay attacks if captured.
if (jwtOptions.TokenLifetime > TimeSpan.FromHours(1))
    throw new InvalidOperationException("The token lifetime should not exceed 1 hour.");

// Add configuration to the service collection to be used in services
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton(databaseConnectionOptions);

// Configure the Kestrel server request limits and server certificate
builder.Services.ConfigureKestrelServerLimitsAndCertificate(serviceAuthOptions.Certificate);

// Configure the authentication and authorization pipelines
builder.Services.ConfigureJwtAuthentication(jwtOptions);
builder.Services.ConfigureJwtAuthorizationRequiresNameIdentifier();

// Configure a rate limiting pipeline to limit the number of requests per IP address
builder.Services.ConfigureRateLimitingPerIp();

// Add services for database access
builder.Services.AddTransient<DatabaseConnectionFactory>();
builder.Services.AddTransient<ConsumptionValueRepository>();
builder.Services.AddTransient<LogRepository>();
builder.Services.AddTransient<UserRepository>();
builder.Services.AddTransient<MeterRepository>();

// Add built-in services for hashing passwords
builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();

// Allow Dapper to map the column names with underscores to the C# properties
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddControllers();

var app = builder.Build();

// Make sure to redirect all requests to HTTPS as the first step in the pipeline.
app.UseHttpsRedirection();

// Make sure the website is never downgraded by an attacker from HTTPS back to HTTP.
app.UseHsts();

// Enable the per IP rate limiter
app.UseRateLimiter();

// Enable the authentication and authorization pipelines for ALL incoming requests as configured above.
// In the context of this smart meter specific webservice no requests should be made unauthenticated or unauthorized.
app.UseAuthentication();
app.UseAuthorization();

// Use ASP.NET Core's controller mapping that automatically maps requests to Controller methods.
app.MapControllers();

await app.RunAsync();