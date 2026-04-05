using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
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

    public async Task<Device?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(d => d.Category)
            .Include(d => d.Department)
            .Include(d => d.CreatedByUser)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<Device>> GetDevicesDueForInspectionAsync(DateOnly deadline, CancellationToken ct = default)
        => await DbSet
            .Where(d => d.NextInspectionDate != null && d.NextInspectionDate <= deadline)
            .Include(d => d.Department)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Device> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        DeviceStatus? status = null,
        Guid? departmentId = null,
        Guid? categoryId = null,
        string? search = null,
        string? sortBy = null,
        string? sortDir = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Include(d => d.Category)
            .Include(d => d.Department)
            .AsQueryable();

        // Filters
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        if (departmentId.HasValue)
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        if (categoryId.HasValue)
            query = query.Where(d => d.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d =>
                d.Name.Contains(search) ||
                d.InventoryNumber.Contains(search) ||
                d.Manufacturer.Contains(search) ||
                d.Model.Contains(search) ||
                (d.SerialNumber != null && d.SerialNumber.Contains(search)));

        var totalCount = await query.CountAsync(ct);

        // Sorting
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => descending ? query.OrderByDescending(d => d.Name) : query.OrderBy(d => d.Name),
            "inventorynumber" => descending ? query.OrderByDescending(d => d.InventoryNumber) : query.OrderBy(d => d.InventoryNumber),
            "manufacturer" => descending ? query.OrderByDescending(d => d.Manufacturer) : query.OrderBy(d => d.Manufacturer),
            "status" => descending ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
            "purchasedate" => descending ? query.OrderByDescending(d => d.PurchaseDate) : query.OrderBy(d => d.PurchaseDate),
            "nextinspectiondate" => descending ? query.OrderByDescending(d => d.NextInspectionDate) : query.OrderBy(d => d.NextInspectionDate),
            "createdat" => descending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            _ => descending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
        };

        // Pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Inspection>> GetInspectionsAsync(Guid deviceId, CancellationToken ct = default)
        => await Context.Inspections
            .Where(i => i.DeviceId == deviceId)
            .OrderByDescending(i => i.InspectionDate)
            .ToListAsync(ct);

    public async Task<Inspection> AddInspectionAsync(Inspection inspection, CancellationToken ct = default)
    {
        await Context.Inspections.AddAsync(inspection, ct);
        return inspection;
    }
}
