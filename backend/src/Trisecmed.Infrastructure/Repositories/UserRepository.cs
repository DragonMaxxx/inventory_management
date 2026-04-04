using Microsoft.EntityFrameworkCore;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TrisecmedDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);

    public async Task<User?> GetByActivationTokenAsync(string token, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.ActivationToken == token, ct);
}
