namespace OperatorSendJob.Configurations;

// Used in services: OperatorSendJob
// Settings for the consumption value send job
public class JobConfig
{
    public const string Key = "Job";
    
    // Defines after how many days old consumption value entries are deleted
    public required int DeleteOlderThanDays { get; init; }
}
