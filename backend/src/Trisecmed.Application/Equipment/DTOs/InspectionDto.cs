namespace Trisecmed.Application.Equipment.DTOs;

public record InspectionDto
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public DateOnly InspectionDate { get; init; }
    public DateOnly? NextInspectionDate { get; init; }
    public string? Result { get; init; }
    public string? Notes { get; init; }
    public string? PerformedBy { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
}
