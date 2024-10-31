namespace OperatorSendJob.Configurations;

public class JobConfig
{
    public const string Key = "Job";
    
    public required int DeleteOlderThanDays { get; init; }
}