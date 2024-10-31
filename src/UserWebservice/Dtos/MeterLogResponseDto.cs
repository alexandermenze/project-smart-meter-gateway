using System.Collections.Immutable;
using Domain.Models;

namespace UserWebservice.Dtos;

public record MeterLogResponseDto(string MeterId, ImmutableArray<LogEntry> Logs);