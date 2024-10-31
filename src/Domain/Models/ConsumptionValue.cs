namespace Domain.Models;

public record ConsumptionValue(
    int Id,
    string MeterId,
    // The timestamp of when the consumption value was written to the database. This is useful as an indicator
    // of connection health between the meter and the database. Delayed consumption value creation could ba a sign
    // of a problem or a malicious attack.
    DateTime Created,
    // The consumption value in kWh during this period.
    // The type decimal in .NET is suitable for representing exact values in the decimal system.
    decimal PeriodValue,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    // The signature is a signed hash of the period start and end, and the period value in the following format:
    // {PeriodValue}_{PeriodStart in ISO 8601 format}_{PeriodEnd in ISO 8601 format}
    // Example: 1.24_2024-10-16T19:15:00.0000000Z_2024-10-16T19:30:00.0000000Z  
    // For a reference implementation, see the SignatureGenerator project.
    string Signature,
    bool SentToOperator);