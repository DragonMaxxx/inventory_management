using Trisecmed.Domain.Enums;

namespace Trisecmed.Application.Equipment.DTOs;

public record DeviceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string InventoryNumber { get; init; } = null!;
    public string? SerialNumber { get; init; }
    public string Manufacturer { get; init; } = null!;
    public string Model { get; init; } = null!;
    public Guid CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public Guid DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public DeviceStatus Status { get; init; }
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public DateOnly? WarrantyExpires { get; init; }
    public DateOnly? NextInspectionDate { get; init; }
    public string? Notes { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
