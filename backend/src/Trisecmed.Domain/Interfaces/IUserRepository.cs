using Trisecmed.Domain.Entities;

namespace Trisecmed.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<User?> GetByActivationTokenAsync(string token, CancellationToken ct = default);
}
