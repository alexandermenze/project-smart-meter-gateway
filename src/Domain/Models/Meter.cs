namespace Domain.Models;

public record Meter(
    string Id,
    // The UserId is the owner of the meter. This is used for data selection and authorization.
    int UserId);