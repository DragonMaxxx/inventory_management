using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;
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
}
