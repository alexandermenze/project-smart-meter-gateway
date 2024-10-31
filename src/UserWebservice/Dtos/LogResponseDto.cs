using System.Collections.Immutable;
using Domain.Models;

namespace UserWebservice.Dtos;

// This class represents the response structure for the log endpoint
public record LogResponseDto(ImmutableArray<LogEntry> UserLogs, ImmutableArray<MeterLogResponseDto> MeterLogs);