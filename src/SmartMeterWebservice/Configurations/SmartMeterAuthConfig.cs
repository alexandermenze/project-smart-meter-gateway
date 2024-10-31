namespace SmartMeterWebservice.Configurations;

public class SmartMeterAuthConfig
{
    public const string Key = "SmartMeterAuth";

    // The common names that are allowed to connect to the smart meter webservice.
    public required IReadOnlyCollection<string> AllowedCommonNames { get; init; }
}