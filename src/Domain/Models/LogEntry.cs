using Microsoft.Extensions.Logging;

namespace Domain.Models;

public record LogEntry(
    int Id,
    // Either the UserId or MeterId should be set, but not both
    int? UserId,
    string? MeterId,
    DateTime Created,
    LogLevel Level,
    // The message is a human-readable description of the event that occurred
    string Message,
    // Source is the name of the service that created the log entry
    string Source);