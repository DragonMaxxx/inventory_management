using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Repositories;

public class DeviceRepository : Repository<Device>, IDeviceRepository
{
    public DeviceRepository(TrisecmedDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Device>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        => await DbSet
            .Include(d => d.Category)
            .Include(d => d.Department)
            .Where(d => d.DepartmentId == departmentId)
            .ToListAsync(ct);

    public async Task<Device?> GetByInventoryNumberAsync(string inventoryNumber, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(d => d.InventoryNumber == inventoryNumber, ct);

    public async Task<IReadOnlyList<Device>> GetDevicesDueForInspectionAsync(DateOnly deadline, CancellationToken ct = default)
        => await DbSet
            .Where(d => d.NextInspectionDate != null && d.NextInspectionDate <= deadline)
            .Include(d => d.Department)
            .ToListAsync(ct);
}
