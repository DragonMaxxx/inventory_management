using Trisecmed.Domain.Enums;

namespace Trisecmed.Domain.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = null!;
    public string InventoryNumber { get; set; } = null!;
    public string? SerialNumber { get; set; }
    public string Manufacturer { get; set; } = null!;
    public string Model { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public DeviceStatus Status { get; set; } = DeviceStatus.Active;

    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateOnly? WarrantyExpires { get; set; }
    public DateOnly? NextInspectionDate { get; set; }
    public string? Notes { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public ICollection<Inspection> Inspections { get; set; } = [];
    public ICollection<Failure> Failures { get; set; } = [];
}
