using Trisecmed.Domain.Enums;

namespace Trisecmed.Application.Failures.DTOs;

public record FailureStatusHistoryDto
{
    public Guid Id { get; init; }
    public FailureStatus OldStatus { get; init; }
    public FailureStatus NewStatus { get; init; }
    public Guid ChangedByUserId { get; init; }
    public string? ChangedByUserName { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
