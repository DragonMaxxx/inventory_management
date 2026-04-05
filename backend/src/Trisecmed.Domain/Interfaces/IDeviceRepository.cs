using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Domain.Interfaces;

public interface IDeviceRepository : IRepository<Device>
{
    Task<IReadOnlyList<Device>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<Device?> GetByInventoryNumberAsync(string inventoryNumber, CancellationToken ct = default);
    Task<Device?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Device>> GetDevicesDueForInspectionAsync(DateOnly deadline, CancellationToken ct = default);

    Task<(IReadOnlyList<Device> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        DeviceStatus? status = null,
        Guid? departmentId = null,
        Guid? categoryId = null,
        string? search = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Inspection>> GetInspectionsAsync(Guid deviceId, CancellationToken ct = default);
    Task<Inspection> AddInspectionAsync(Inspection inspection, CancellationToken ct = default);
}
