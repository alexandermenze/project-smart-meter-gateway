using Database.Repositories;
using Domain.Models;
using Microsoft.Extensions.Logging;
using OperatorSendJob.Configurations;

namespace OperatorSendJob;

// This class encapsulates the logic to delete old consumption values from the database to 
// improve data minimization and reduce the risk of data breaches.
public class DeleteJob(
    JobConfig jobConfig,
    LogRepository logRepository,
    ConsumptionValueRepository consumptionValueRepository)
{
    public async Task Run()
    {
        var olderThan = DateTime.UtcNow.AddDays(-jobConfig.DeleteOlderThanDays);
        var consumptionValues = await consumptionValueRepository.GetSentOlderThan(olderThan);

        foreach (var value in consumptionValues)
        {
            // Using a try-catch prevents the application from crashing if an exception is thrown for one value.
            // The processing will continue with the next value and retry this one the next time the job runs.
            try
            {
                await consumptionValueRepository.Delete(value);
                
                await logRepository.Insert(new LogEntry(
                    0,
                    UserId: null,
                    value.MeterId,
                    DateTime.Now,
                    LogLevel.Information,
                    $"Deleted value for period {value.PeriodStart:O} - {value.PeriodEnd:O} from database.",
                    nameof(DeleteJob)));
            }
            catch (Exception ex)
            {
                // More detailed exception information is logged in the console message for debugging purposes.
                // Using output collectors these information can be stored, extracted and analyzed.
                // As log entries are indicators for the users, they don't contain detailed error information.
                // This ensures that possible attackers don't get any information about the system.
                Console.WriteLine("Failed to delete value: {0}", ex.Message);
                
                await logRepository.Insert(new LogEntry(
                    0,
                    UserId: null,
                    value.MeterId,
                    DateTime.UtcNow,
                    LogLevel.Error,
                    $"Failed to delete value for period {value.PeriodStart:O} - {value.PeriodEnd:O}.",
                    nameof(DeleteJob)));
            }
        }
    }
}