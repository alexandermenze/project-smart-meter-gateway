using System.Net.Http.Json;
using Database.Repositories;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace OperatorSendJob;

public class SendJob(
    IHttpClientFactory httpClientFactory,
    LogRepository logRepository,
    ConsumptionValueRepository consumptionValueRepository)
{
    public async Task Run()
    {
        var consumptionValues = await consumptionValueRepository.GetUnsent();

        foreach (var value in consumptionValues)
        {
            // Using a try-catch prevents the application from crashing if an exception is thrown for one value.
            // The processing will continue with the next value and retry this one the next time the job runs.
            try
            {
                await SendValueToOperator(value);
            }
            catch (Exception ex)
            {
                // More detailed exception information is logged in the console message for debugging purposes.
                // Using an output collector these information can be stored, extracted and analyzed.
                // As log entries are indicators for the users, they don't contain detailed error information.
                // This ensures that possible attackers don't get any information about the system.
                Console.WriteLine("Failed to send value to operator: {0}", ex.Message);

                await logRepository.Insert(new LogEntry(
                    0,
                    UserId: null,
                    value.MeterId,
                    DateTime.Now,
                    LogLevel.Error,
                    $"Failed to send value for period {value.PeriodStart:O} - {value.PeriodEnd:O} to operator.",
                    nameof(SendJob)));
            }
        }
    }

    private async Task SendValueToOperator(ConsumptionValue consumptionValue)
    {
        await PostValueToOperatorApi(consumptionValue);

        // After the value has been sent to the operator, the sent_to_operator flag is set to true to avoid
        // re-processing that entry again in the future.
        // If the service crashes or the database connection is lost in between, the value is sent again.
        // This type of transaction ensures At-Least-Once delivery.
        // If the values is sent multiple times the operator service has to handle this.
        await consumptionValueRepository.SetSentToOperator(consumptionValue);

        await logRepository.Insert(new LogEntry(
            0,
            UserId: null,
            consumptionValue.MeterId,
            DateTime.Now,
            LogLevel.Information,
            // While the time frame should be a good indicator for the user,
            // the actual value is not logged to avoid data leakage.
            $"Sent value for time frame {consumptionValue.PeriodStart:O} - {consumptionValue.PeriodEnd:O} to operator.",
            nameof(SendJob)));
    }

    private async Task PostValueToOperatorApi(ConsumptionValue consumptionValue)
    {
        // Receive the named HttpClient configured in Program.cs
        using var client = httpClientFactory.CreateClient("operator");

        var response = await client.PostAsJsonAsync("api/consumptionValues", consumptionValue);

        // Every failure should throw an exception to ensure that the value is not marked as sent.
        response.EnsureSuccessStatusCode();
    }
}