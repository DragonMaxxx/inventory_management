using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Repositories;

public class FailureRepository : Repository<Failure>, IFailureRepository
{
    public FailureRepository(TrisecmedDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Failure>> GetByDeviceAsync(Guid deviceId, CancellationToken ct = default)
        => await DbSet
            .Include(f => f.ReportedByUser)
            .Include(f => f.ServiceProvider)
            .Where(f => f.DeviceId == deviceId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

    public async Task<Failure?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(f => f.Device)
            .Include(f => f.ReportedByUser)
            .Include(f => f.Department)
            .Include(f => f.ServiceProvider)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<(IReadOnlyList<Failure> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        FailureStatus? status, FailurePriority? priority,
        Guid? departmentId, Guid? deviceId,
        string? search, string? sortBy, string? sortDir,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Include(f => f.Device)
            .Include(f => f.ReportedByUser)
            .Include(f => f.Department)
            .Include(f => f.ServiceProvider)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);
        if (priority.HasValue)
            query = query.Where(f => f.Priority == priority.Value);
        if (departmentId.HasValue)
            query = query.Where(f => f.DepartmentId == departmentId.Value);
        if (deviceId.HasValue)
            query = query.Where(f => f.DeviceId == deviceId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f =>
                f.Description.Contains(search) ||
                f.Device.Name.Contains(search) ||
                f.Device.InventoryNumber.Contains(search));

        var totalCount = await query.CountAsync(ct);

        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy?.ToLowerInvariant() switch
        {
            "status" => descending ? query.OrderByDescending(f => f.Status) : query.OrderBy(f => f.Status),
            "priority" => descending ? query.OrderByDescending(f => f.Priority) : query.OrderBy(f => f.Priority),
            "device" => descending ? query.OrderByDescending(f => f.Device.Name) : query.OrderBy(f => f.Device.Name),
            "resolvedat" => descending ? query.OrderByDescending(f => f.ResolvedAt) : query.OrderBy(f => f.ResolvedAt),
            "createdat" => descending ? query.OrderByDescending(f => f.CreatedAt) : query.OrderBy(f => f.CreatedAt),
            _ => descending ? query.OrderByDescending(f => f.CreatedAt) : query.OrderBy(f => f.CreatedAt),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<FailureStatusHistory>> GetStatusHistoryAsync(Guid failureId, CancellationToken ct = default)
        => await Context.Set<FailureStatusHistory>()
            .Include(h => h.ChangedByUser)
            .Where(h => h.FailureId == failureId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);

    public async Task<FailureStatusHistory> AddStatusHistoryAsync(FailureStatusHistory history, CancellationToken ct = default)
    {
        await Context.Set<FailureStatusHistory>().AddAsync(history, ct);
        return history;
    }
}
