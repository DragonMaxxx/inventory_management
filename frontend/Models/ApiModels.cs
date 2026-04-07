using System.Text.Json.Serialization;

namespace Trisecmed.Web.Models;

public record LoginRequest(string Email, string Password);

public record LoginResponse
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; init; } = "";
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; init; } = "";
}

public record PagedResult<T>
{
    [JsonPropertyName("items")] public List<T> Items { get; init; } = [];
    [JsonPropertyName("totalCount")] public int TotalCount { get; init; }
    [JsonPropertyName("page")] public int Page { get; init; }
    [JsonPropertyName("pageSize")] public int PageSize { get; init; }
    [JsonPropertyName("totalPages")] public int TotalPages { get; init; }
}

public record DeviceDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("inventoryNumber")] public string InventoryNumber { get; init; } = "";
    [JsonPropertyName("serialNumber")] public string? SerialNumber { get; init; }
    [JsonPropertyName("manufacturer")] public string Manufacturer { get; init; } = "";
    [JsonPropertyName("model")] public string Model { get; init; } = "";
    [JsonPropertyName("categoryId")] public Guid CategoryId { get; init; }
    [JsonPropertyName("categoryName")] public string? CategoryName { get; init; }
    [JsonPropertyName("departmentId")] public Guid DepartmentId { get; init; }
    [JsonPropertyName("departmentName")] public string? DepartmentName { get; init; }
    [JsonPropertyName("status")] public string Status { get; init; } = "";
    [JsonPropertyName("purchaseDate")] public string? PurchaseDate { get; init; }
    [JsonPropertyName("purchasePrice")] public decimal? PurchasePrice { get; init; }
    [JsonPropertyName("warrantyExpires")] public string? WarrantyExpires { get; init; }
    [JsonPropertyName("nextInspectionDate")] public string? NextInspectionDate { get; init; }
    [JsonPropertyName("notes")] public string? Notes { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
}

public record FailureDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("deviceId")] public Guid DeviceId { get; init; }
    [JsonPropertyName("deviceName")] public string? DeviceName { get; init; }
    [JsonPropertyName("deviceInventoryNumber")] public string? DeviceInventoryNumber { get; init; }
    [JsonPropertyName("reportedByUserName")] public string? ReportedByUserName { get; init; }
    [JsonPropertyName("departmentName")] public string? DepartmentName { get; init; }
    [JsonPropertyName("description")] public string Description { get; init; } = "";
    [JsonPropertyName("status")] public string Status { get; init; } = "";
    [JsonPropertyName("priority")] public string Priority { get; init; } = "";
    [JsonPropertyName("serviceProviderName")] public string? ServiceProviderName { get; init; }
    [JsonPropertyName("repairCost")] public decimal? RepairCost { get; init; }
    [JsonPropertyName("resolvedAt")] public DateTime? ResolvedAt { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
}

public record CategoryDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("description")] public string? Description { get; init; }
}

public record DepartmentDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("code")] public string? Code { get; init; }
}

public record InspectionDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("deviceId")] public Guid DeviceId { get; init; }
    [JsonPropertyName("inspectionDate")] public string InspectionDate { get; init; } = "";
    [JsonPropertyName("nextInspectionDate")] public string? NextInspectionDate { get; init; }
    [JsonPropertyName("result")] public string? Result { get; init; }
    [JsonPropertyName("notes")] public string? Notes { get; init; }
    [JsonPropertyName("performedBy")] public string? PerformedBy { get; init; }
}

public record FailureStatusHistoryDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("failureId")] public Guid FailureId { get; init; }
    [JsonPropertyName("oldStatus")] public string? OldStatus { get; init; }
    [JsonPropertyName("newStatus")] public string NewStatus { get; init; } = "";
    [JsonPropertyName("changedByUserName")] public string? ChangedByUserName { get; init; }
    [JsonPropertyName("notes")] public string? Notes { get; init; }
    [JsonPropertyName("changedAt")] public DateTime ChangedAt { get; init; }
}

public class InspectionFormModel
{
    public DateTime? InspectionDate { get; set; } = DateTime.Today;
    public DateTime? NextInspectionDate { get; set; }
    public string? Result { get; set; }
    public string? Notes { get; set; }
    public string? PerformedBy { get; set; }
}

public record NotificationDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("type")] public string Type { get; init; } = "";
    [JsonPropertyName("subject")] public string Subject { get; init; } = "";
    [JsonPropertyName("body")] public string? Body { get; init; }
    [JsonPropertyName("recipientEmail")] public string RecipientEmail { get; init; } = "";
    [JsonPropertyName("isSent")] public bool IsSent { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; init; }
}

public record UserDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("email")] public string Email { get; init; } = "";
    [JsonPropertyName("firstName")] public string FirstName { get; init; } = "";
    [JsonPropertyName("lastName")] public string LastName { get; init; } = "";
    [JsonPropertyName("role")] public string Role { get; init; } = "";
    [JsonPropertyName("departmentId")] public Guid? DepartmentId { get; init; }
    [JsonPropertyName("departmentName")] public string? DepartmentName { get; init; }
    [JsonPropertyName("isActive")] public bool IsActive { get; init; }
}

public class UserFormModel
{
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "EquipmentWorker";
    public Guid? DepartmentId { get; set; }
}

// ── Form models ──

public class DeviceFormModel
{
    public string Name { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public string? SerialNumber { get; set; }
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public Guid? CategoryId { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyExpires { get; set; }
    public DateTime? NextInspectionDate { get; set; }
    public string? Notes { get; set; }
}

public class FailureFormModel
{
    public Guid? DeviceId { get; set; }
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "Medium";
}

public class CategoryFormModel
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class DepartmentFormModel
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
}

public class ServiceProviderFormModel
{
    public string Name { get; set; } = "";
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxId { get; set; }
}

public record ServiceProviderDto
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("contactPerson")] public string? ContactPerson { get; init; }
    [JsonPropertyName("email")] public string? Email { get; init; }
    [JsonPropertyName("phone")] public string? Phone { get; init; }
    [JsonPropertyName("taxId")] public string? TaxId { get; init; }
}
