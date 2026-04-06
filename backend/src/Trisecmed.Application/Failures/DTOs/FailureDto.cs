using Trisecmed.Domain.Enums;

namespace Trisecmed.Application.Failures.DTOs;

public record FailureDto
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public string? DeviceName { get; init; }
    public string? DeviceInventoryNumber { get; init; }
    public Guid ReportedByUserId { get; init; }
    public string? ReportedByUserName { get; init; }
    public Guid DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public string Description { get; init; } = null!;
    public FailureStatus Status { get; init; }
    public FailurePriority Priority { get; init; }
    public Guid? ServiceProviderId { get; init; }
    public string? ServiceProviderName { get; init; }
    public decimal? RepairCost { get; init; }
    public string? RepairDescription { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
