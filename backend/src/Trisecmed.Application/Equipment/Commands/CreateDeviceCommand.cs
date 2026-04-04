using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Application.Equipment.Commands;

public record CreateDeviceCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = null!;
    public string InventoryNumber { get; init; } = null!;
    public string? SerialNumber { get; init; }
    public string Manufacturer { get; init; } = null!;
    public string Model { get; init; } = null!;
    public Guid CategoryId { get; init; }
    public Guid DepartmentId { get; init; }
    public DeviceStatus Status { get; init; } = DeviceStatus.Active;
    public DateOnly? PurchaseDate { get; init; }
    public decimal? PurchasePrice { get; init; }
    public DateOnly? WarrantyExpires { get; init; }
    public DateOnly? NextInspectionDate { get; init; }
    public string? Notes { get; init; }
}
