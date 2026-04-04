using Trisecmed.Domain.Interfaces;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TrisecmedDbContext _context;

    public UnitOfWork(TrisecmedDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
}
