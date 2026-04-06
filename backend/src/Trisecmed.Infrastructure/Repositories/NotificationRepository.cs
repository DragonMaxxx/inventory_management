using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(TrisecmedDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? type, bool? isSent, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(n => n.Type == type);
        if (isSent.HasValue)
            query = query.Where(n => n.IsSent == isSent.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
