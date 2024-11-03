namespace SmartMeterWebservice.Configurations;

// Used in services: SmartMeterWebservice
// Configurations for the smart meter authentication
public class SmartMeterAuthConfig
{
    public const string Key = "SmartMeterAuth";

    // The common names that are authorized to connect to the smart meter webservice.
    public required IReadOnlyCollection<string> AllowedCommonNames { get; init; }
}
