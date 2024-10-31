namespace SmartMeterWebservice.Dtos;

// This class is used to represent the incoming request from the smart meters to save a consumption value.
// The signature is stored as a base64 encoded string.
public record ConsumptionValueRequest(decimal PeriodValue, DateTime PeriodStart, DateTime PeriodEnd, string Signature);