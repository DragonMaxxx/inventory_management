using Trisecmed.Domain.Entities;

namespace Trisecmed.Application.Failures.DTOs;

public static class FailureMappings
{
    public static FailureDto ToDto(this Failure f) => new()
    {
        Id = f.Id,
        DeviceId = f.DeviceId,
        DeviceName = f.Device?.Name,
        DeviceInventoryNumber = f.Device?.InventoryNumber,
        ReportedByUserId = f.ReportedByUserId,
        ReportedByUserName = f.ReportedByUser != null ? $"{f.ReportedByUser.FirstName} {f.ReportedByUser.LastName}" : null,
        DepartmentId = f.DepartmentId,
        DepartmentName = f.Department?.Name,
        Description = f.Description,
        Status = f.Status,
        Priority = f.Priority,
        ServiceProviderId = f.ServiceProviderId,
        ServiceProviderName = f.ServiceProvider?.Name,
        RepairCost = f.RepairCost,
        RepairDescription = f.RepairDescription,
        ResolvedAt = f.ResolvedAt,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt,
    };

    public static FailureStatusHistoryDto ToDto(this FailureStatusHistory h) => new()
    {
        Id = h.Id,
        OldStatus = h.OldStatus,
        NewStatus = h.NewStatus,
        ChangedByUserId = h.ChangedByUserId,
        ChangedByUserName = h.ChangedByUser != null ? $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}" : null,
        Notes = h.Notes,
        CreatedAt = h.CreatedAt,
    };
}
