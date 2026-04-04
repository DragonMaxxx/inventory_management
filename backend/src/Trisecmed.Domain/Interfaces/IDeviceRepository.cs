using Trisecmed.Domain.Entities;

namespace Trisecmed.Domain.Interfaces;

public interface IDeviceRepository : IRepository<Device>
{
    Task<IReadOnlyList<Device>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<Device?> GetByInventoryNumberAsync(string inventoryNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Device>> GetDevicesDueForInspectionAsync(DateOnly deadline, CancellationToken ct = default);
}
