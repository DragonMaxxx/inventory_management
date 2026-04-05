using Trisecmed.Domain.Entities;

namespace Trisecmed.Application.Equipment.DTOs;

public static class DeviceMappings
{
    public static DeviceDto ToDto(this Device d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        InventoryNumber = d.InventoryNumber,
        SerialNumber = d.SerialNumber,
        Manufacturer = d.Manufacturer,
        Model = d.Model,
        CategoryId = d.CategoryId,
        CategoryName = d.Category?.Name,
        DepartmentId = d.DepartmentId,
        DepartmentName = d.Department?.Name,
        Status = d.Status,
        PurchaseDate = d.PurchaseDate,
        PurchasePrice = d.PurchasePrice,
        WarrantyExpires = d.WarrantyExpires,
        NextInspectionDate = d.NextInspectionDate,
        Notes = d.Notes,
        CreatedByUserId = d.CreatedByUserId,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt,
    };

    public static InspectionDto ToDto(this Inspection i) => new()
    {
        Id = i.Id,
        DeviceId = i.DeviceId,
        InspectionDate = i.InspectionDate,
        NextInspectionDate = i.NextInspectionDate,
        Result = i.Result,
        Notes = i.Notes,
        PerformedBy = i.PerformedBy,
        CreatedByUserId = i.CreatedByUserId,
        CreatedAt = i.CreatedAt,
    };
}
