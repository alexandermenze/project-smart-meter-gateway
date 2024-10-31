using Certificates.Validation;
using Dapper;
using Database.Connections;
using Database.Repositories;
using Domain.Options;
using Domain.Parsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OperatorSendJob;
using OperatorSendJob.Configurations;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

ConfigurationParser.From(config)
    .Get(JobConfig.Key, out JobConfig jobConfig)
    .Get(OperatorApiConfig.Key, out OperatorApiConfig operatorApiConfig)
    .Map(out DatabaseConnectionOptions databaseConnectionOptions)
    .Map(out ServiceAuthOptions serviceAuthOptions);

var serviceCollection = new ServiceCollection();

// Register the required configuration to be used in the jobs
serviceCollection.AddSingleton(jobConfig);
serviceCollection.AddSingleton(databaseConnectionOptions);

// This configures a named HttpClient with the base URL of the operator API and the client certificate.
serviceCollection
    .AddHttpClient("operator", httpClient => { httpClient.BaseAddress = new Uri(operatorApiConfig.BaseUrl); })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        // Add the client certificate to the request and ensure that the certificate is automatically presented to the server.
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(serviceAuthOptions.Certificate);
        handler.ClientCertificateOptions = ClientCertificateOption.Automatic;

        // Validate the operator API server certificate with the trusted root certificates.
        handler.ServerCertificateCustomValidationCallback = (_, cert, chain, _) =>
            CertificateValidation.ValidateSignedByCaWithCommonName(serviceAuthOptions.TrustedRootCertificates, cert,
                chain, [operatorApiConfig.CommonName]);

        return handler;
    });

// Allow Dapper to map the column names with underscores to the C# properties
DefaultTypeMap.MatchNamesWithUnderscores = true;

// Register the database dependencies
serviceCollection.AddTransient<DatabaseConnectionFactory>();
serviceCollection.AddTransient<ConsumptionValueRepository>();
serviceCollection.AddTransient<LogRepository>();

// Register the jobs for dependency injection
serviceCollection.AddTransient<SendJob>();
serviceCollection.AddTransient<DeleteJob>();

var serviceProvider = serviceCollection.BuildServiceProvider();

// The TryAndLog method is used to catch exceptions in the jobs and log them to the console.
// This way the application won't crash if an exception is thrown. All jobs will be executed.
await TryAndLog(nameof(SendJob), () => serviceProvider.GetRequiredService<SendJob>().Run());
await TryAndLog(nameof(DeleteJob), () => serviceProvider.GetRequiredService<DeleteJob>().Run());

return;

async Task TryAndLog(string name, Func<Task> action)
{
    try
    {
        await action();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred in {name}: {ex.Message}");
    }
}